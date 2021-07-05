using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace Azure.Reaper.Lib
{
    public class PowerSchedule
    {

        // List of tags from the resource and resource group merged together
        public Dictionary<string, string> Tags;

        // The powerstate of the resource
        private string _powerState;

        // The region that the resource is running in
        private string _regionName;

        // List of settings
        private IEnumerable<Setting> _settings;
        private IEnumerable<LocationTZ> _timeZones;

        // States whether the resource should be started or not
        private bool startResource = false;
        private bool stopResource = false;

        // State the start and stop times of the resource
        public TimeSpan tsStart;
        public TimeSpan tsStop;
        public bool PermittedToRunOnDay = false;

        // Logger object
        private ILog _logger;

        public TimeZoneInfo zoneInfo = TimeZoneInfo.Utc;

        // Define the regular expression pattern which will be used to determine the
        // start and stop time from the tag value
        private Regex pattern = new Regex(@"((?:[01]\d|2[0-3]):?(?:[0-5]\d))\s*?-\s*?((?:[01]\d|2[0-3]):?(?:[0-5]\d))");

        private DateTime _timeNowUTC;
        private DateTime _timzoneNow;

        /// <summary>
        /// PowerSchedule is used to determine if the specified resource should be running or not
        /// The constructor takes the tags of the resource group and the resource itself
        /// </summary>
        /// <param name="rgTags">Tags on the resource group</param>
        /// <param name="resTags">Tags on the resource</param>
        /// <param name="powerState">Current powerstate of the resource</param>
        public PowerSchedule(
            List<Setting> settings,
            IEnumerable<LocationTZ> timeZones,
            ILog logger,
            DateTime timeNowUTC,
            IReadOnlyDictionary<string, string> rgTags,
            IReadOnlyDictionary<string, string> resTags,
            string powerState,
            string regionName)
        {
            // merge the tags of the group and the resource
            // the resource tags need to overwrite those from the group as they
            // have higher priority
            Tags = (Dictionary<string, string>) rgTags;

            MergeTags(rgTags, resTags);
            _powerState = powerState.ToLower();
            _regionName = regionName;
            _settings = settings;
            _timeZones = timeZones;
            _logger = logger;
            _timeNowUTC = timeNowUTC;

            // using the settings, set the start and stop timespans
            // to be use in future calculations
            string startTime = settings.First(s => s.name == "vm_start").value;
            string stopTime = settings.First(s => s.name == "vm_stop").value;

            tsStart = DateTime.ParseExact(startTime, "HHmm", CultureInfo.InvariantCulture).TimeOfDay;
            tsStop = DateTime.ParseExact(stopTime, "HHmm", CultureInfo.InvariantCulture).TimeOfDay;
        }

        public void MergeTags(
            IReadOnlyDictionary<string, string> rgTags,
            IReadOnlyDictionary<string, string> resTags
        )
        {
            Tags = (Dictionary<string, string>) rgTags;

            // iterate over the resTags and add them, or overwrite the tags
            foreach (KeyValuePair<string, string> item in resTags)
            {
                Tags[item.Key] = item.Value;
            }            
        }

        /// <summary>
        /// States if the resource should be started or not
        /// </summary>
        /// <returns></returns>
        public bool StartResource()
        {
            return startResource;
        }

        /// <summary>
        /// States if the resource should be stopped or not
        /// </summary>
        /// <returns></returns>
        public bool StopResource()
        {
            return stopResource;
        }        

        public void Process()
        {

            // Get the schedule from the tags
            ScheduleFromTags();

            // Determine the timezone
            SetTimeZone();

            // Determine if the machine should be running on this day or not
            PermittedToRunDay();

            ShouldBeRunning();

        }

        public void ShouldBeRunning()
        {
            bool shouldBeRunning = false;

            // using the timespans and the permitted to run result, determine if the
            // machine should be running or not
            if ((_timzoneNow.TimeOfDay > tsStart &&
                 _timzoneNow.TimeOfDay < tsStop &&
                 PermittedToRunOnDay))
            {
                shouldBeRunning = true;
            }

            // determine if the resource should be started or stopped
            // using the powerstate that has been supplied to the class
            Regex regexStopped = new Regex("(?:powerstate/)?deallocated");
            if (regexStopped.IsMatch(_powerState) && shouldBeRunning) {
                startResource = true;
            }

            Regex regexRunning = new Regex("(?:powerstate/)?running");
            if (regexRunning.IsMatch(_powerState) && !shouldBeRunning) {
                stopResource = true;
            }

        }

        /// <summary>
        /// Determine if the resource has the tag set on it
        /// </summary>
        /// <param name="settingName">Name of the setting that contains the name of the tag</param>
        /// <returns></returns>
        public bool HasTag(string settingName)
        {
            bool result = false;

            // get the name of the tag from the settings
            string tagName = _settings.First(s => s.name == settingName).value;

            // determine if the Tags contains the tagName
            if (Tags.ContainsKey(tagName))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Get the value from the tag that has been set if it exists
        /// </summary>
        /// <param name="settingName">Name of the setting that hold the name of the tag</param>
        /// <returns></returns>
        public string GetTag(string settingName)
        {
            string result = string.Empty;

            if (HasTag(settingName))
            {
                // get the name of the tag from the settings
                string tagName = _settings.First(s => s.name == settingName).value;
                result = Tags[tagName];
            }

            return result;
        }

        /// <summary>
        /// Determine if there is a tag set that states the schedule that should be used for the machines
        /// </summary>
        public void ScheduleFromTags()
        {
            // initialise method with some properties
            bool result = false;
            string settingName = "tag_vm_start_stop_time";

            // determine if the tag has been set
            result = HasTag(settingName);

            // if the tag has been found then extract the necessary information
            if (result)
            {
                // get the value of the tag
                string schedule = GetTag(settingName);

                // Ensure that the string matches the regular expression
                // If it does then get the schedule from the tag
                if (pattern.IsMatch(schedule))
                {
                    // create the message to be used in the log
                    string message = String.Format(
                        "Power state schedule found: {0}",
                        schedule
                    );

                    _logger.Information(message);

                    // Split the schedule string into the start and stop times
                    // and then trim the string to make the necessary times
                    string[] parts = schedule.Split("-").ToArray();
                    tsStart = DateTime.ParseExact(parts[0].Trim().Replace(":", ""), "HHmm", CultureInfo.InvariantCulture).TimeOfDay;
                    tsStop = DateTime.ParseExact(parts[1].Trim().Replace(":", ""), "HHmm", CultureInfo.InvariantCulture).TimeOfDay;
                }
                else
                {
                    _logger.Error("Power state schedule has an invalid format: ", schedule);
                }
            }

        }

        /// <summary>
        /// Set the timezone that the schedule should work in from the region of the resource
        /// or the tag (if one has been set)
        /// </summary>
        public void SetTimeZone()
        {
            // get the id of the zone
            string zoneId = String.Empty;

            // determine if the resource has a timezone tag set on it
            string tagZone = GetTag("tag_timezone");

            // if the tagZone is not empty, get the zone from that
            // otherwise use the resource region to get the zone
            if (!string.IsNullOrEmpty(tagZone))
            {
                zoneId = tagZone;
            }
            else
            {
                // Find the TimeZoneInfo based on the regionName
                LocationTZ resourceZone = _timeZones.First(t => t.name == _regionName);
                zoneId = resourceZone.tzId;
            }

            // zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            zoneInfo = TimeZoneConverter.TZConvert.GetTimeZoneInfo(zoneId);

            // work out the offset to be used for calculating times, based on the 
            // specified timezone of the resource
            _timzoneNow = TimeZoneInfo.ConvertTimeFromUtc(_timeNowUTC, zoneInfo);
        }

        /// <summary>
        /// Determine if the resource should be running on the specified day
        /// in the timezone of the resource
        /// </summary>
        /// <param name="timenow">The time now, in the timezone of the resource</param>
        public void PermittedToRunDay()
        {
            // get the permitted days from settings
            string days = _settings.First(s => s.name == "permitted_days").value;

            // Attempt to get the tag that specifies the days of the week from 
            // the resource
            if (HasTag("tag_days_of_week"))
            {
                days = GetTag("tag_days_of_week");
                _logger.Information("Resource contains permitted days for operation: {0}", days);
            }

            // Turn the permitted days into an array so that the current day can be searched for
            string[] permittedDays = days.Split(",").ToArray();

            // get the current day from the timenow
            int currentDay = (int) _timzoneNow.DayOfWeek;

            // if the current days is in the list then allow the macine to run
            if (permittedDays.Contains(currentDay.ToString()))
            {
                PermittedToRunOnDay = true;
            }
            else
            {
                _logger.Information("Resource is not permitted to run on a {0}", _timzoneNow.DayOfWeek);
            }

        }



    }
}