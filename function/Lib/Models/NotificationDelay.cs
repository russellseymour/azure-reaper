using Azure.Reaper.Lib.Interfaces;
using System;

namespace Azure.Reaper.Lib.Models
{
    /// <summary>
    /// Reaper can contact people over an instant messenger, such as Slack
    /// However it will notify each time the Reaper runs, which is 10 mins by default
    /// The NotificationDelay informs Reaper that the target of the message does not want to
    /// be notified for x minutes
    /// </summary>
    public class NotificationDelay : Model<NotificationDelay>, IModel
    {

        /// <summary>
        /// Name of the group for which notifications should be delayed
        /// </summary>
        /// <value></value>
        public string group_name { get; set; }

        public string name { get; set; }

        public string type { get; set; }

        public string subscription_id { get; set; }

        public DateTime last_notified { get; set; }

        public NotificationDelay(IBackend backend, ILog logger) : base(backend, logger, Constants.Table_Name_NotificationDelay)
        {
        }

        public NotificationDelay() : base(Constants.Table_Name_NotificationDelay)
        {
        }

        public void SetKeys()
        {
            PartitionKey = tableName;
            RowKey = subscription_id;
        }

    }
}