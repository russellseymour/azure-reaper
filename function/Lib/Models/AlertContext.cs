using System;
using Newtonsoft.Json;

namespace Azure.Reaper.Lib.Models
{
    public class AlertContext
    {
        // public ActivityLog activityLog;

        public Claims _claims;
        public Authorization authorization;
        
        public DateTime eventTimestamp;

        public Properties properties;

        public string subStatus;

        public string caller;

        public string claims;
        public AlertContext()
        {
            this.ReadClaims();
        }

        public void ReadClaims() {
            // remove the encoding from the claims string
            string json = Azure.Reaper.Utilities.RemoveEncoding(this.claims);
            this._claims = JsonConvert.DeserializeObject<Claims>(json);
        }

        /// <summary>
        /// State if the current item has been craeted or not
        /// This is done by testing to see if subStatus is equal to "created"
        /// </summary>
        /// <returns>bool</returns>
        public bool IsCreated()
        {
            bool result = false;

            if (subStatus.ToLower() == "created")
            {
                result = true;
            }

            return result;
        }

        public string GetResourceGroupName() {
            return properties._responseBody.name;
        }

        public string GetSubscriptionId() {
            return authorization.scope.Split('/')[2];
        }

        public string GetValue(string name)
        {
            string result = String.Empty;
            switch (name)
            {
                case "tag_owner":
                    result = _claims.name;
                    break;
                case "tag_owner_email":
                    result = caller;
                    break;
                case "tag_date":
                    result = eventTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                break;
            }

            return result;
        }        
    }
}