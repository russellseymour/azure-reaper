using System;
using Xunit;
using System.Collections.Generic;
using Azure.Reaper.Lib;

namespace Tests
{

    public class PowerScheduleTests : IClassFixture<TestsFixture>
    {
        private const string POWER_STATE = "powerstate/running";
        private const string REGION = "ukwest";
        private const string TIMEZONE = "GMT Standard Time";
        TestsFixture data;

        public PowerScheduleTests(TestsFixture data)
        {
            this.data = data;
        }

        [Fact]
        public void MergeTagsNoConflicts()
        {

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            groupTags.Add("ownerEmail", "me@example.com");

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // Get the merged tags
            Dictionary<string, string> tags = powerSchedule.Tags;

            // assert that there is one item and it is the ownerEmail
            Assert.Equal(1, tags.Count);
            Assert.Contains("ownerEmail", tags.Keys);
            Assert.Equal("me@example.com", tags["ownerEmail"]);
        }

        [Fact]
        public void MergeTagsWithConflicts()
        {
            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            groupTags.Add("ownerEmail", "me@example.com");
            resourceTags.Add("ownerEmail", "example@me.com");

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // Get the merged tags
            Dictionary<string, string> tags = powerSchedule.Tags;

            // assert that there is one item and it is the ownerEmail
            Assert.Equal(1, tags.Count);
            Assert.Contains("ownerEmail", tags.Keys);
            Assert.Equal("example@me.com", tags["ownerEmail"]);            
        }

        [Fact]
        public void HasAndGetTag()
        {
            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            groupTags.Add("STARTSTOPTIME", "09:00 - 2000");

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            Assert.True(powerSchedule.HasTag("tag_vm_start_stop_time"));
            Assert.Equal("09:00 - 2000", powerSchedule.GetTag("tag_vm_start_stop_time"));
        }

        [Fact]
        public void GetSchedule()
        {

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            groupTags.Add("STARTSTOPTIME", "09:00 - 2000");

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // get the schedule from the tags
            powerSchedule.ScheduleFromTags();

            // create the timespan to measure against
            TimeSpan startTime = new TimeSpan(9, 0, 0);
            TimeSpan stopTime = new TimeSpan(20, 0, 0);

            // assert that the start and stop times are correct
            Assert.Equal(startTime, powerSchedule.tsStart);
            Assert.Equal(stopTime, powerSchedule.tsStop);
        }

        [Fact]
        /// <summary>
        /// This will get set the timezone based on the region of the resource
        /// </summary>
        public void GetTimeZoneFromRegion() {
            
            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            powerSchedule.SetTimeZone();

            // ensure that the zoneInfo is correctly set
            // TimeZoneInfo zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE);
            TimeZoneInfo zoneInfo = TimeZoneConverter.TZConvert.GetTimeZoneInfo(TIMEZONE);
            Assert.Equal(zoneInfo, powerSchedule.zoneInfo);
        }

        [Fact]
        /// <summary>
        /// Test that the timezone can be set by applying a Tag to the resource
        /// </summary>
        public void GetTimeZoneFromTag()
        {
            string TEST_TIMEZONE_ID = "Mountain Standard Time";

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            // Add the tag in to state the timezone that should be used
            resourceTags.Add("TIMEZONE", TEST_TIMEZONE_ID);

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                DateTime.UtcNow,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            powerSchedule.SetTimeZone();

            // ensure that the zoneInfo is correctly set
            // TimeZoneInfo zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TEST_TIMEZONE_ID);
            TimeZoneInfo zoneInfo = TimeZoneConverter.TZConvert.GetTimeZoneInfo(TEST_TIMEZONE_ID);
            Assert.Equal(zoneInfo, powerSchedule.zoneInfo);
        }

        [Fact]
        public void ResourcePermittedToRunGMT() 
        {
            // set the time and the timezone to work with
            DateTime timeReference = new DateTime(2021, 07, 02); 

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                timeReference,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // ensure that the resource should be running now
            powerSchedule.SetTimeZone();
            powerSchedule.PermittedToRunDay();

            Assert.True(powerSchedule.PermittedToRunOnDay);
        }

        [Fact]
        public void ResourceNotPermittedToRunGMT() 
        {
            // set the time and the timezone to work with
            DateTime timeReference = new DateTime(2021, 07, 03); 

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                timeReference,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // ensure that the resource should be running Now
            powerSchedule.SetTimeZone();
            powerSchedule.PermittedToRunDay();

            Assert.False(powerSchedule.PermittedToRunOnDay);
        }        

        [Fact]
        public void ResourcePermittedToRunMountainTime()
        {
            string TEST_TIMEZONE_ID = "Mountain Standard Time";

            // set the time and the timezone to work with
            DateTime timeReference = new DateTime(2021, 07, 03, 0, 1, 0); 

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            // Add the tag in to state the timezone that should be used
            resourceTags.Add("TIMEZONE", TEST_TIMEZONE_ID);

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                timeReference,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // ensure that the resource should be running now
            powerSchedule.SetTimeZone();
            powerSchedule.PermittedToRunDay();

            Assert.True(powerSchedule.PermittedToRunOnDay);            
        }

        [Fact]
        public void ResourceShouldBeStopped()
        {
            // set the time and the timezone to work with
            DateTime timeReference = new DateTime(2021, 07, 02, 21, 0, 0); 

            // create the tags for the resource group and the 
            Dictionary<string, string> groupTags = new Dictionary<string, string>();
            Dictionary<string, string> resourceTags = new Dictionary<string, string>();

            PowerSchedule powerSchedule = new PowerSchedule(
                data.settings,
                data.timeZones,
                data.logger,
                timeReference,
                groupTags,
                resourceTags,
                POWER_STATE,
                REGION
            );

            // ensure that the resource should be running now
            powerSchedule.SetTimeZone();
            powerSchedule.PermittedToRunDay();
            powerSchedule.ShouldBeRunning();

            Assert.True(powerSchedule.StopResource());
        }
    }
}
