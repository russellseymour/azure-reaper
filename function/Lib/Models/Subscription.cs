using Azure.Reaper.Lib.Interfaces;

namespace Azure.Reaper.Lib.Models
{
    public class Subscription : Model<Subscription>, IModel
    {
        /// <summary>
        /// Short name for the subscription.
        /// This is used to search for the subscription in the table so should not contain any spaces
        /// or other punctuation
        /// </summary>
        /// <value></value>
        public string name { get; set; }

        /// <summary>
        /// ID of the subscription
        /// </summary>
        /// <value></value>
        public string subscription_id { get; set; }

        /// <summary>
        /// Client ID of the SPN that has been setup for Azure Reaper to access the subscription
        /// to perform the power management and deletion of resources
        /// </summary>
        /// <value></value>
        public string client_id { get; set; }

        /// <summary>
        /// Secret for the specified client id of the SPN
        /// </summary>
        /// <value></value>
        public string client_secret { get; set; }

        /// <summary>
        /// Tenant ID for the subscription
        /// </summary>
        /// <value></value>
        public string tenant_id { get; set; }

        /// <summary>
        /// State if the subscription is enable in the function
        /// This controls whether the tagger engine operates on the resource groups
        /// </summary>
        /// <value></value>
        public bool enabled { get; set; }

        /// <summary>
        /// State whether the reaper is enabled on the subscription
        /// If set to true, Reaper will manage the power of the appropriate resources in
        /// a group and delete when they have expired.
        /// </summary>
        /// <value></value>
        public bool reaper { get; set; }

        /// <summary>
        /// Dry run allows Reaper to be enabled for the subscription, but is not
        /// destructive - rather it will show the intention. This is useful for testing
        /// to ensure that Reaper is working as expected
        /// </summary>
        /// <value></value>
        public bool dryrun { get; set; }        
        
        public Subscription(IBackend backend, ILog logger) : base(backend, logger, Constants.Table_Name_Subscriptions)
        {
        }

        public Subscription() : base(Constants.Table_Name_Subscriptions)
        {
        }

        public void SetKeys()
        {
            PartitionKey = tableName;
            RowKey = subscription_id;
        }

        public bool IsDisabled()
        {
            return !enabled;
        }
    }
}