namespace Azure.Reaper.Lib.Models
{
    public class Authorization
    {
        public string action;
        public string scope;

        public Authorization() {}

        /// <summary>
        /// Returns the subscription ID from the authorization scope
        /// </summary>
        /// <returns></returns>
        public string GetSubscriptionId()
        {
            string subscriptionId;

            // Split the scope string using the / character and extract the second element
            subscriptionId = scope.Split('/')[1];

            return subscriptionId;
        }
    }
}