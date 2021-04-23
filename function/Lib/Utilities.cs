using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Interfaces;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Azure.Reaper
{
  public class Utilities
  {
    public static IAzure AzureLogin (
      Subscription subscription,
      AzureEnvironment azureEnvironment,
      ILog logger
    )
    {
      logger.Debug("Attempting to authenticate for subscription: {subscription}", (string) subscription.subscription_id);

      // Create service principal
      ServicePrincipalLoginInformation spn = new ServicePrincipalLoginInformation {
        ClientId = subscription.client_id,
        ClientSecret = subscription.client_secret
      };
    
      // Create the Azure credentials
      AzureCredentials azureCredential = new AzureCredentials(
        spn,
        subscription.tenant_id,
        azureEnvironment
      );

      // Create and return the Azure object
      IAzure azure =  Microsoft.Azure.Management.Fluent.Azure
                      .Configure()
                      .Authenticate(azureCredential)
                      .WithSubscription(subscription.subscription_id);

      return azure;
    }

        /// <summary>
        /// Helper function to determine if the string that has been passed to the app
        /// is valid JSON
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static bool IsValidJson(string payload)
        {
            var trimmed = payload.Trim();
            bool result = true;

            if ((trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                (trimmed.StartsWith("[") && trimmed.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(trimmed);
                }
                catch (JsonReaderException)
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

      /// <summary>
      /// Helper function to see if the delay for notifications has expired
      /// </summary>
      /// <param name="timestamp">Time that the previous notification occured</param>
      /// <param name="delay">Time on seconds that the delay should have exceeded</param>
      /// <returns>boolean</returns>
      public static bool DelayExpired(DateTime timestamp, int delay)
      {       
        return DelayExpired(timestamp, delay, DateTime.UtcNow);
      }

      public static bool DelayExpired(DateTime timestamp, int delay, DateTime reference)
      {
        bool result = false;

        // determine the elapsed seconds since the last notification
        int elapsed = (reference - timestamp).Seconds;

        result = elapsed > delay;

        return result;
      }
  }
}