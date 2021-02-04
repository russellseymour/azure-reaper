using System;
using System.Text;
using Newtonsoft.Json;

namespace Azure.Reaper.Models
{

  public class ActivityLog
  {

    public string subStatus;
    public DateTime eventTimestamp;
    public string resourceGroupName;
    public string subscriptionId;
    public string caller;
    public ActivityLogClaims claim;
    public string claims;

    public ActivityLog()
    {
      this.ReadClaims();
    }

    public void ReadClaims() {
      // remove the encoding from the claims string
      string json = this.RemoveEncoding(this.claims);
      this.claim = JsonConvert.DeserializeObject<ActivityLogClaims>(json);
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

    public string GetValue(string name)
    {
      string result = String.Empty;
      switch (name)
      {
        case "tag_owner":
          result = claim.name;
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

    private string RemoveEncoding(string encodedJson)
    {
      var sb = new StringBuilder(encodedJson);
      sb.Replace("\\", string.Empty);
      sb.Replace("\"[", "[");
      sb.Replace("]\"", "]");
      return sb.ToString();

    }
  }
}