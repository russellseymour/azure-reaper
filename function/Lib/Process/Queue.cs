using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Resources;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Collections.Generic;

namespace Azure.Reaper.Lib.Process
{
    public class Queue
    {

        private ActivityLog activityLog;

        private IBackend _backend;
        private ILog _logger;

        private Subscription subscription;

        public Queue(QueueMessage message, IBackend backend, ILog logger)
        {
            _backend = backend;
            _logger = logger;

            // Get the activityLog message from the main queue message
            activityLog = message.data.context.activityLog;
        }

        /// <summary>
        /// Process the message that has been sent to the queue
        /// The subscription that the resource group has been created in will be
        /// checked against the list of subscriptions in the database
        /// If it can be found and is enabled then the tagging of the resource group will be performed
        /// </summary>
        public void Process()
        {

            // Only proceed if a resource group has been created
            if (activityLog.IsCreated())
            {

                // read the claims in the message
                activityLog.ReadClaims();

                _logger.Information("Considering Resource Group: {name}", activityLog.resourceGroupName);

                // Attempt to get the named subscription from the backend
                // This is used to see if the subscription is being monitored as well as checking
                // that reaper is enabled on it
                Subscription sub = new Subscription(_backend, _logger);
                Response result = sub.GetByName(activityLog.subscriptionId);

                subscription = (Subscription) result.GetData();

                // check to see if a subscription has been found, if it is enabled
                // and if running in dry run mode
                if (subscription == null)
                {
                    _logger.Warning("Subscription is not known to Reaper: {subscription}", activityLog.subscriptionId);
                }
                else if (subscription.IsDisabled())
                {
                    _logger.Warning("Tagging for this subscription is not enabled: {name} [{subscriptionId}]", subscription.name, activityLog.subscriptionId);
                }
                else
                {
                    performTagging();
                }
            }
        }

        private async void performTagging()
        {
            _logger.Information("Attempting to tag resource group: {name}", activityLog.resourceGroupName);

            // Login to azure using the credentials that have been set on the subscription
            IAzure azure = Utilities.AzureLogin(subscription, AzureEnvironment.AzureGlobalCloud, _logger);

            // Check to see if the resource group exists in the subscription
            bool exists = await azure.ResourceGroups.ContainAsync(activityLog.resourceGroupName);

            if (exists)
            {
                
                // Get a list of the tags that need to be added to the group
                Setting setting = new Setting(_backend, _logger);
                Response response = setting.GetByCategory("tags");
                List<Setting> settings = response.GetData();

                _logger.Debug("Found '{tags}' tags to be considered for the resource group", settings.Count);

                // Retrieve the Resource Group object from Azure
                IResourceGroup resourceGroup = azure.ResourceGroups.GetByName(activityLog.resourceGroupName);
                Group rg = new Group(
                    _backend,
                    _logger,
                    resourceGroup,
                    settings,
                    activityLog
                );

                // Add the tags to the resource group
                rg.AddDefaultTags();

            }
            else
            {
                _logger.Error("Unable to find resource group: {name}", activityLog.resourceGroupName);
            }
        }
    }
}