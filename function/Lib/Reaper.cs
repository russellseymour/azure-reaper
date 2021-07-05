using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Resources;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ContainerService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Azure.Reaper.Lib
{
    public class Reaper
    {
        // Define class properties
        private ILog _logger;
        private NotificationDelay _notificationDelay;
        private IBackend _backend;
        private IAzure azure;
        private List<Models.Setting> settings;
        private IEnumerable<LocationTZ> timezones;
        private DateTime timeNowUtc;
        private Models.PowerStatistics powerStats;

        public Reaper(IBackend backend, ILog logger, List<Models.Setting> settings, NotificationDelay notificationDelay)
        {
            _backend = backend;
            _logger = logger;
            _notificationDelay = notificationDelay;
            this.settings = settings;
        }

        public bool Process(List<Models.Subscription> subscriptions, Models.LocationTZ locationTZ)
        {

            // bool error = false;
            // Uri webhook_url = null;

            // Create a list of resource group candidates that may be deleted
            List<string> resource_group_candidates = new List<string>();

            // Define a variable to accept the response from SlackClient
            HttpResponseMessage response = new HttpResponseMessage();
            
            bool destroy = Convert.ToBoolean(settings.First(s => s.name == "destroy").value);
            bool manage_vms = Convert.ToBoolean(settings.First(s => s.name == "manage_vms").value);

            // get a list of the timezones to work with
            timezones = locationTZ.GetByName().GetData();

            // Create a connection to Slack
            // ensure that the webhook url can be turned into a URI
            /*
            try
            {
                webhook_url = new Uri(settings.First(s => s.name == "webhook_url").value);
            }
            catch
            {
                string msg = (String.Format("Unable to create a URI from `webhook_url`: {0}", settings.First(s => s.name == "webhook_url").value));
                log.LogInformation(msg);
                error = true;
            }
            */

            // if (!error)
            // {
                // SlackClient slackClient = new SlackClient(
                //     webhook_url, 
                //     settings.First(s => s.name == "bot_username").value,
                //     settings.First(s => s.name == "icon_url").value,
                //     settings.First(s => s.name == "token").value, 
                //     log,
                //     Convert.ToBoolean(settings.First(s => s.name == "slack_enabled").value)
                // );

                // // Get all the subscriptions that have the reaper enabled on them
                // IEntity subscription = new Subscription(client, log);
                // // dynamic subscriptions = subscription.Get(subscription);
                // IEnumerable<Subscription> subscriptions = subscription.GetUsingSQL("SELECT * FROM subscriptions t WHERE t.enabled = true");

                // // Get al the timezones
                // LocationTZ timezone = new LocationTZ(client, log);
                // IEnumerable<LocationTZ> timezones = timezone.GetAll();

                // Define timestamp to work against so it is the same time for all checks
                // DateTime timeNowUtc = DateTime.UtcNow;

                // Create a NotificationDelay object to use to
                // get previous notification alerts and add new ones
                // NotificationDelay notificationDelay = new NotificationDelay(client, log);

                // iterate around the subscriptions and check the resource groups and vms in each
                foreach (Subscription sub in subscriptions)
                {

                    // only proceed if the reaper has been enabled on the subscription
                    /*
                    if (!sub.reaper) {
                        _logger.Information("Reaper is not enabled on subscription: {name} [{id}]", sub.name, sub.subscription_id);
                        continue;
                    }
                    */

                    // Determine the timenow to be used as criteria
                    // This is done on each subscription because there might be a lot of groups to enumerate
                    timeNowUtc = DateTime.UtcNow;
                    string time_now = timeNowUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");

                    // Login to Azure with the credentials for this subscription
                    azure = Utilities.AzureLogin(sub, AzureEnvironment.AzureGlobalCloud, _logger);

                    // get a list of resource groups in the subscription
                    IEnumerable<IResourceGroup> resource_groups = azure.ResourceGroups.List();

                    // iterate around the resource groups
                    foreach (IResourceGroup resource_group in resource_groups)
                    {

                        Resources.Group rg = new Resources.Group(
                            resource_group,
                            _backend,
                            _logger,
                            settings,
                            _notificationDelay,
                            sub.subscription_id,
                            timezones,
                            timeNowUtc
                        );

                        // Determine if the resource group has expired or not
                        bool notify = rg.ShouldNotify();
                        bool expired = rg.ShouldDelete();

                        _logger.Information(
                            "Considering {group}: Notify = {notify}, Delete = {delete}",
                            resource_group.Name,
                            notify.ToString(),
                            expired.ToString()
                        );

                        /*
                        if (notify)
                        {
                            // Attempt to get the slack userid for the email address
                            //slackClient.GetUserIdByEmail(rg.emailAddress);

                            // Set the properties of the slack message
                            //slackClient.AddField("Name", resource_group.Name);
                            //slackClient.AddField("Expiry Date", rg.expiryDate.ToString("yyyy-MM-ddTHH:mm:ss"));

                            // Add the message to the slack client
                            //slackClient.AddAttachmentItem(rg.message, rg.level);

                            // Send the slack message
                            //response = await slackClient.SendMessageAsync();

                            // Set a record in the notification delay that this resource group has had
                            // a notification sent out
                            NotificationDelay nd = new NotificationDelay();
                            nd.subscription_id = sub.subscription_id;
                            nd.group_name = resource_group.Name;
                            nd.type = "resource_group";

                            await _notificationDelay.Upsert(nd);
                        }
                        */

                        // If the group has expired, delete it
                        // Otherwise work out what machines need to be powered on or off
                        if (expired) 
                        {
                            azure.ResourceGroups.DeleteByNameAsync(resource_group.Name);
                        }
                        else
                        {

                            // Initialise the powerstatistics to be used for notifications
                            powerStats = new PowerStatistics();

                            // So that the Slack user is not inundated with messages, ensure that all machines that have been
                            // found to be in violation of running times are bundled up
                            // List<string> stopped = new List<string>();
                            // List<string> started = new List<string>();

/*
                            // as the resource group is valid look at the virtual machines that are running
                            // and determine if they should be shutdown or not
                            IEnumerable<IVirtualMachine> vms = azure.VirtualMachines.ListByResourceGroup(resource_group.Name);

                            foreach (IVirtualMachine vm in vms)
                            {
                                // create a virtualMachine object to work with
                                VirtualMachine virtualMachine = new VirtualMachine(
                                    sub.subscription_id,
                                    vm,
                                    _logger,
                                    settings,
                                    _notificationDelay,
                                    rg,
                                    timezones,
                                    timeNowUtc
                                );

                                // Set the power state of the machine
                                VirtualMachine.Status powerStatus = virtualMachine.SetPowerState();

                                // Using the powerStatus determine what has been started and what has been stopped
                                if (powerStatus == VirtualMachine.Status.Started && !virtualMachine.NotifiedWithinTimePeriod())
                                {
                                    started.Add(virtualMachine.GetName());
                                }

                                if (powerStatus == VirtualMachine.Status.Stopped && !virtualMachine.NotifiedWithinTimePeriod())
                                {
                                    stopped.Add(virtualMachine.GetName());
                                }

                                // Add a notification for this machine
                                /*
                                NotificationDelay nd = new NotificationDelay();
                                nd.subscription_id = sub.subscription_id;
                                nd.group_name = resource_group.Name;
                                nd.type = "virtual_machine";
                                nd.name = virtualMachine.GetName();

                                await _notificationDelay.Upsert(nd);
                                
                                
                            }
                            */

                            manageVirtualMachines(resource_group);

                            manageK8sClusters(resource_group);

                            // only send out messages for VMs if there are some
                            //if (vms.Count() > 0 && (started.Count > 0 || stopped.Count > 0))
                            // {

                            //     // Define the properties of the slack message to send
                            //     slackClient.AddField("Name", resource_group.Name);
                            //     slackClient.AddField("VMs Started", String.Join("\n", started));
                            //     slackClient.AddField("VMs Stopped", String.Join("\n", stopped));

                            //     // Define the message that needs to be attached
                            //     string message = "The following machines have been stopped or started.";
                            //     if (!manage_vms)
                            //     {
                            //         message += " Reaper is not currently running in active mode, no machines have been stopped or started.";
                            //     }

                            //     slackClient.AddAttachmentItem(message, "warning");

                            //     // Send a slack message to the owner
                            //     response = await slackClient.SendMessageAsync();
                            // }
                        }
                    }
                }
            // }
 
            return true;
        }

        /// <summary>
        /// Find the Kubernetes clusters in the specified resource group and determine if they need to be
        /// left alone, shutdown or startedup
        /// </summary>
        /// <param name="rgName">Name of hte resource group to look for clusters in</param>
        private void manageK8sClusters(IResourceGroup rg) {

            // determine if any kubernetes clusters exist in the group
            IEnumerable<IKubernetesCluster> clusters = azure.KubernetesClusters.ListByResourceGroup(rg.Name);

            if (clusters.Count() == 0) {
                _logger.Information("No AKS clusters in group: {0}", rg.Name);
            } else {

                _logger.Information("Analysing AKS Clusters: {0}", clusters.Count().ToString());

                // iterate around the clusters
                foreach (IKubernetesCluster cluster in clusters) {

                    string operation = String.Empty;
                    string powerState = cluster.Inner.PowerState.Code.Value;

                    // Determine if the cluster should be running or not
                    PowerSchedule powerSchedule = new PowerSchedule(
                        settings, 
                        timezones, 
                        _logger,
                        timeNowUtc,
                        rg.Tags, 
                        cluster.Tags, 
                        powerState, 
                        cluster.RegionName
                    );

                    powerSchedule.Process();

                    // use the powerschedule information to determine if the resource should be running
                    if (powerSchedule.StartResource())
                    {
                        powerStats.StartResource("aks", cluster.Name);
                        operation = "start";
                    }

                    if (powerSchedule.StopResource())
                    {
                        powerStats.StopResource("aks", cluster.Name);
                        operation = "stop";
                    }

                    if (!String.IsNullOrEmpty(operation))
                    {
                        // build up the URL
                        object[] replacements = { cluster.Manager.RestClient.BaseUri, cluster.Manager.SubscriptionId, rg.Name, cluster.Name, operation };
                        string url = String.Format("{0}subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.ContainerService/managedClusters/{3}/{4}?api-version=2021-05-01", replacements);

                        _logger.Information(url);

                        // Create a new HttpClient top 
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                        CancellationToken cancellationToken = new CancellationToken();
                        HttpClient httpClient = new HttpClient();

                        cluster.Manager.RestClient.Credentials.ProcessHttpRequestAsync(request, cancellationToken).GetAwaiter().GetResult();
                        HttpResponseMessage response = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).GetAwaiter().GetResult();
                        
                        _logger.Information(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }

        /// <summary>
        /// Find the virtual machines in the group and determine what ones need to have
        /// the powerstate modified
        /// </summary>
        /// <param name="rg"></param>
        private void manageVirtualMachines(IResourceGroup rg) {

            // get a list of the virtual machines in the group
            IEnumerable<IVirtualMachine> virtualMachines = azure.VirtualMachines.ListByResourceGroup(rg.Name);

            // check the beginning of the name of hte resource group
            // if it starts with MC_ then it will be a managed cluster and Reaper should not deal
            // with the virtual machines directly
            if (rg.Name.ToLower().StartsWith("mc_"))
            {
                _logger.Information("Not managing Virtual Machines in managed AKS cluster: {0}", rg.Name);
                return;
            }

            if (virtualMachines.Count() == 0)
            {
                _logger.Information("No Virtual Machines in group: {0}", rg.Name);
                return;
            }

            _logger.Information("Analysing Virtual Machines: {0}", virtualMachines.Count().ToString());

            // Determine if VMs are to be managed, return if they are not
            bool manage_vms = Convert.ToBoolean(settings.First(s => s.name == "manage_vms").value);
            if (!manage_vms) {
                _logger.Information("Reaper is not set to manage virtual machines, please check your settings");
                return;
            }

            foreach (IVirtualMachine vm in virtualMachines)
            {
                // get the powerstate of the vm
                string powerstate = vm.PowerState.ToString();

                // Create a powerschedule object to determine if the machine should be running or not
                PowerSchedule powerSchedule = new PowerSchedule(
                    settings,
                    timezones,
                    _logger,
                    timeNowUtc,
                    rg.Tags,
                    vm.Tags,
                    powerstate,
                    vm.RegionName
                );

                powerSchedule.Process();

                // start or stop the resource based on the results from the powerschedule
                if (powerSchedule.StartResource())
                {
                    powerStats.StartResource("vm", vm.Name);
                    _logger.Information("[{0}] Starting virtual machine: {1}", rg.Name, vm.Name);
                    vm.StartAsync();
                }

                if (powerSchedule.StopResource())
                {
                    powerStats.StopResource("vm", vm.Name);
                    _logger.Information("[{0}] Stopping virtual machine: {1}", rg.Name, vm.Name);
                    vm.DeallocateAsync();
                }

            }
        }
    }
}
