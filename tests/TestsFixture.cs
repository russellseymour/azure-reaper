
using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;
using System;
using System.Collections.Generic;
// using Xunit;

public class TestsFixture : IDisposable
{

    public List<Setting> settings;
    public List<LocationTZ> timeZones;
    public ILog logger;

    // Global initialisation
    public TestsFixture()
    {
        // Create a new logger for the tests
        logger = new Azure.Reaper.Lib.Loggers.AppSerilog();

        // configure settings for the tests
        // these are to emulate the data that is retrieved from the database when running as a function
        settings = new List<Setting>();

        // add in the necessary settings for the tests
        // -- start stop tag
        Setting setting1 = new Setting();
        setting1.name = "tag_vm_start_stop_time";
        setting1.value = "STARTSTOPTIME";
        settings.Add(setting1);

        // -- name of the tag for setting the timezone
        Setting setting2 = new Setting();
        setting2.name = "tag_timezone";
        setting2.value = "TIMEZONE";
        settings.Add(setting2);

        // -- set the days that resources are permitted to run on
        Setting setting3 = new Setting();
        setting3.name = "permitted_days";
        setting3.value = "1,2,3,4,5";
        settings.Add(setting3);
        
        // -- add the tag that holds the days of the week the resource is
        // allowed to run on
        Setting setting4 = new Setting();
        setting4.name = "tag_days_of_week";
        setting4.value = "DAYSOFWEEK";
        settings.Add(setting4);

        // -- add the settings for the start and stop times of resources
        Setting setting5 = new Setting();
        setting5.name = "vm_start";
        setting5.value = "0800";
        settings.Add(setting5);

        Setting setting6 = new Setting();
        setting6.name = "vm_stop";
        setting6.value = "1800";
        settings.Add(setting6);

        Setting setting7 = new Setting();
        setting7.name = "tag_inuse";
        setting7.value = "InUse";
        settings.Add(setting7);        

        // configure necessary timezones
        timeZones = new List<LocationTZ>();

        LocationTZ tz1 = new LocationTZ();
        tz1.name = "ukwest";
        tz1.tzId = "GMT Standard Time";
        timeZones.Add(tz1);

        LocationTZ tz2 = new LocationTZ();
        tz2.name = "westcentralus";
        tz2.tzId = "Mountain Standard Time";
        timeZones.Add(tz2);

    }

    // Global teardown
    public void Dispose()
    {

    }
}