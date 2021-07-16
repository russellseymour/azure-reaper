using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Resources;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;

namespace Azure.Reaper.Lib.Process
{
    public class Queue
    {

        private AlertContext alertContext;

        private IBackend _backend;
        private ILog _logger;

        private Subscription subscription;

        public Queue(QueueMessage message, IBackend backend, ILog logger)
        {
            _backend = backend;
            _logger = logger;

            // Get the activityLog message from the main queue message
            alertContext = message.data.alertContext; // .context.activityLog;
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
            if (alertContext.IsCreated())
            {

                // read the claims in the message
                alertContext.ReadClaims();
                alertContext.properties.ReadResponseBody();

                _logger.Information("Considering Resource Group: {name}", alertContext.GetResourceGroupName());

                // Attempt to get the named subscription from the backend
                // This is used to see if the subscription is being monitored as well as checking
                // that reaper is enabled on it
                Subscription sub = new Subscription(_backend, _logger);
                Response result = sub.GetByName(alertContext.GetSubscriptionId());

                subscription = (Subscription) result.GetData();

                // check to see if a subscription has been found, if it is enabled
                // and if running in dry run mode
                if (subscription == null)
                {
                    _logger.Warning("Subscription is not known to Reaper: {subscription}", alertContext.GetSubscriptionId());
                }
                else if (subscription.IsDisabled())
                {
                    _logger.Warning("Tagging for this subscription is not enabled: {name} [{subscriptionId}]", subscription.name, alertContext.GetSubscriptionId());
                }
                else
                {
                    performTagging();
                }
            }
        }

        private async void performTagging()
        {
            string rgName = alertContext.GetResourceGroupName();
            _logger.Information("Attempting to tag resource group: {name}", rgName);

            // Login to azure using the credentials that have been set on the subscription
            IAzure azure = Utilities.AzureLogin(subscription, AzureEnvironment.AzureGlobalCloud, _logger);

            // Check to see if the resource group exists in the subscription
            try {
                bool exists = await azure.ResourceGroups.ContainAsync(rgName);

                if (exists)
                {
                    
                    // Get a list of the tags that need to be added to the group
                    Setting setting = new Setting(_backend, _logger);
                    Response response = setting.GetByCategory("tags");
                    List<Setting> settings = response.GetData();

                    _logger.Debug("Found '{tags}' tags to be considered for the resource group", settings.Count);

                    // Retrieve the Resource Group object from Azure
                    IResourceGroup resourceGroup = azure.ResourceGroups.GetByName(rgName);
                    Group rg = new Group(
                        _backend,
                        _logger,
                        resourceGroup,
                        settings,
                        alertContext
                    );

                    // Add the tags to the resource group
                    rg.AddDefaultTags();

                }
                else
                {
                    _logger.Error("Unable to find resource group: {name}", rgName);
                }
            } catch (CloudException ex) {
                _logger.Fatal("Unable to access subscription '{subscription}', check that the Service Principal for this subscription correct and have the right permissions ({error})", subscription.subscription_id, ex.Message);
            } catch (Exception ex) {
                _logger.Fatal("An error occurred: {message}", ex.Message);
            }
        }
    }
}