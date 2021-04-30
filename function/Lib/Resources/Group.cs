using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Interfaces;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Azure.Reaper.Lib.Resources
{
    public class Group
    {
    // Initialise property to hold the resource group from Azure
    private IResourceGroup resourceGroup;

    private IBackend _backend;
    private ILog _logger;
    private List<Setting> settings;
    private NotificationDelay notificationDelay;
    private string subscriptionId;
    private LocationTZ timezone;
    public string emailAddress = String.Empty;
    public DateTime expiryDate;
    public string message;
    public string level;
    private DateTime timeNowUtc;
    private Models.AlertContext alertContext;

    public Group(
      IResourceGroup resourceGroup,
      IBackend backend,
      ILog logger,
      List<Setting> settings,
      NotificationDelay notificationDelay,
      string subscriptionId,
      LocationTZ timezone,
      DateTime timeNowUtc
    )
    {
      this.resourceGroup = resourceGroup;
      this._backend = backend;
      this._logger = logger;
      this.settings = settings;
      this.notificationDelay = notificationDelay;
      this.subscriptionId = subscriptionId;
      this.timezone = timezone;
      this.timeNowUtc = timeNowUtc;
    }

    public Group(
      IBackend backend,
      ILog logger,
      IResourceGroup resourceGroup,
      List<Setting> settings,
      Models.AlertContext alertContext
    )
    {
      this.resourceGroup = resourceGroup;
      this._backend = backend;
      this._logger = logger;
      this.settings = settings;
      this.alertContext = alertContext;
    }

    /// <summary>
    /// Determine if the group has had any tags assigned to ir
    /// </summary>
    /// <returns>bool</returns>
    public bool HasTags()
    {
      bool result = false;

      if (resourceGroup.Inner.Tags == null)
      {
        _logger.Information("No tags have been set: {group}", resourceGroup.Name);
      }
      else
      {
        result = true;
      }

      return result;
    }

    /// <summary>
    /// Determine if the group has been assigned the 'InUse' tag
    /// </summary>
    /// <returns>bool</returns>
    public bool InUse()
    {
      bool result = false;

      // ensure that the group has tags
      bool hasTags = HasTags();

      // If the tags include InUse attempt tp convert it and then perform the test
      if (hasTags && resourceGroup.Inner.Tags.ContainsKey("InUse"))
      {
        bool inUse = Convert.ToBoolean(resourceGroup.Inner.Tags["InUse"]);

        if (inUse)
        {
          _logger.Information("Group is in use, ignoring: {group}", resourceGroup.Name);
          result = true;
        }

      }

      return result;
    }

    /// <summary>
    /// Ensures that the tags include the tag_owner_email tag
    /// Also determines if the notifications email list is set, and if it is whether the specified
    ///   email address in the tag is part of the list
    /// Then it checks to see if a notification delay exists and then whether a notification is
    ///   due or not
    /// Finally the expiration date for the resource group is checked against the settings
    /// </summary>
    /// <returns>bool</returns>
    public bool ShouldNotify()
    {
      bool result = false;

      bool notified = NotifiedWithinTimePeriod();

      // If the group has not had a previous notification, determine if it is within
      // the duration period allowed for a resource group
      if (!notified)
      {
        if (HasTag("tag_owner_email"))
        {
          string emailAddress = GetTag("tag_owner_email");
          bool isMember = IsMember(emailAddress);

          // if not a member then clear the email address
          if (isMember)
          {
            this.emailAddress = (string) emailAddress;
            result = true;
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Return the name of the resource group
    /// </summary>
    /// <returns>string</returns>
    public string GetName()
    {
      return resourceGroup.Name;
    }

    /// <summary>
    /// Determine if the group has the named tag
    /// </summary>
    /// <param name="name"></param>
    /// <returns>bool</returns>
    public bool HasTag(string name)
    {
      bool result = false;
      string tagName =  settings.First(s => s.name == name).value;

      if (resourceGroup.Inner.Tags != null && resourceGroup.Inner.Tags.ContainsKey(tagName))
      {
        result = true;
      }

      return result;
    }

    public string GetTag(string name)
    {
      string result = String.Empty;

      // Determine that the tag exists
      if (HasTag(name))
      {
        string tagName =  settings.First(s => s.name == name).value;
        result = resourceGroup.Inner.Tags[tagName];
      }

      return result;
    }

    public Dictionary<string, string> GetTags()
    {
      return (Dictionary<string, string>) resourceGroup.Inner.Tags;
    }

    /// <summary>
    /// Determine if the specified email address is a member of the notification_emails
    /// setting, it is has been specified
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns>bool</returns>
    private bool IsMember(string emailAddress)
    {
      bool result = true;
      if (!String.IsNullOrEmpty(emailAddress))
      {
        // Create a list of the emails that are in settings
        string emails = (string) settings.First(s => s.name == "notification_emails").value;
        List<string> emailAddresses = new List<string>(emails.Split(','));

        // if the list is not empty and the email address is not in the list, set the result to false
        if (emailAddresses.Count > 0 && !emailAddresses.Contains((string) emailAddress))
        {
          _logger.Information("Notifications are currently disabled for: {0}", (string) emailAddress);
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
    /// Determine if the resource group has previously been notified
    /// The last_notified value is used against the current time to check that it has not
    /// had notifications within the duration in settings
    /// </summary>
    /// <returns></returns>
    private bool NotifiedWithinTimePeriod()
    {
      bool result = false;

      // Use a notification object to determine if the group already exists in the table
      // for this time period
      // NotificationDelay previousNotification = (NotificationDelay) notificationDelay.Get(
      //   new string[] { subscriptionId, resourceGroup.Name, "resourceGroup" },
      //   new string[] { "subscription", "group_name", "type" }
      // );

      // Create a dictionary of fields and values for the query
      Dictionary<string, dynamic> criteria = new Dictionary<string, dynamic>();
      criteria.Add("subscription_id", subscriptionId);
      criteria.Add("group_name", resourceGroup.Name);
      criteria.Add("type", "resourceGroup");

      NotificationDelay nd = new NotificationDelay(_backend, _logger);
      Response response = nd.Get(criteria);
      dynamic data = response.GetData();

      if (data.Count == 1)
      {
        // determine how many seconds have elapsed since the last notification
        //int elapsed = (DateTime.UtcNow - previousNotification.last_notified).Seconds;

        // get the notification delay
        //int notifyDelay = Convert.ToInt32(settings.First(s => s.name == "notify_delay").value);

        //result = elapsed > notifyDelay;
        NotificationDelay previousNotification = (NotificationDelay) data.First();

        int delay = Convert.ToInt32(settings.First(s => s.name == "notify_delay").value);
        result = Utilities.DelayExpired(previousNotification.last_notified, delay, timeNowUtc);

        // if (result)
        // {
        //   _logger.Debug("Last notified {elapsed} seconds ago: {group}", elapsed.ToString(), resourceGroup.Name);
        // }
      }

      return result;
    }

    /// <summary>
    /// Determine if the group has expired and therefore if it should be deleted
    /// Uses the duration in days from settings to determine if the RG should be deleted
    /// All calculations are performed in UTC
    /// </summary>
    /// <returns></returns>
    public bool ShouldDelete()
    {
      bool result = false;

      // Do not delete if the group is flagged as being InUse
      if (!InUse())
      {

        TimeZoneInfo zone = TimeZoneInfo.Utc;

        // Get the destroy mode
        bool destroy = Convert.ToBoolean(settings.First(s => s.name == "destroy").value);

        // Get the timezones
        Response response = timezone.GetByName();
        IEnumerable<LocationTZ> timezones = response.GetData();

        if (HasTag("tag_date"))
        {
          // Get the created date of the rg in UTC
          DateTime rgCreateDateUtc = DateTime.SpecifyKind(DateTime.Parse(GetTag("tag_date")), DateTimeKind.Utc);

          // Determine the expiry date
          Double rgLifetimeDuration = Convert.ToDouble(settings.First(s => s.name == "duration").value);
          expiryDate = rgCreateDateUtc.AddDays(rgLifetimeDuration);

          // compare the expiry date against the time now to determine if it is expired
          if (timeNowUtc > expiryDate)
          {
            result = true && destroy;

            // set the message to display in the slack notification
            if (destroy)
            {
              message = "Your resource group has expired and will be deleted";
              level = "danger";
            }
            else
            {
              message = "Your resource group has expired and would be delete. Reaper is not running in destructive mode";
              level = "warning";
            }
          }
        }
      }

      return result;
    }

    public void AddDefaultTags()
    {
      dynamic valueOfTag;
      string nameOfTag;
      bool update = false;

      // Create a dictionary of the group tags
      Dictionary<string, string> groupTags;

      _logger.Information("Adding default tags to resource group: {group}", GetName());

      // Determine if the group has any tags, if not create them
      // Otherwise retrieve them from the group so they cxan be updated
      if (!HasTags())
      {
        groupTags = new Dictionary<string, string>();
      }
      else
      {
        groupTags = GetTags();
      }

      // Create array of tags to check on the resource group
      ArrayList compulsoryTags = new ArrayList() {"tag_owner", "tag_owner_email", "tag_date" };

      // iterate around the compulsorytags and ensure each exists on the resource group
      foreach (string compulsoryTag in compulsoryTags)
      {

        // Determine if the tag_owner tag is present
        valueOfTag = HasTag(compulsoryTag);
        nameOfTag = settings.First(s => s.name == compulsoryTag).value;
        if (!valueOfTag)
        {
          // Get the value from the ActivityLog
          string value = alertContext.GetValue(compulsoryTag);

          _logger.Information("Adding '{tag}' tag: group", nameOfTag, GetName());
          groupTags.Add(nameOfTag, value);
          update = true;
        }
      }

      // Update the resource group, if additions have been made
      if (update)
      {
        _logger.Information("Updating: {group}", GetName());
        resourceGroup.Update().WithTags(groupTags).Apply();
      }
    }
  }        
}