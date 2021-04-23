using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Reaper.Lib.Process
{

    public class Timer
    {

        private string _timerName;
        private IBackend _backend;
        private ILog _logger;

        public Timer(IBackend backend, ILog logger, string timerName)
        {
            _backend = backend;
            _logger = logger;
            _timerName = timerName;
        }

        public async void Process()
        {

            // switch on the timerName to select the correct operation to run
            switch (_timerName)
            {
                case Constants.Timer_Reaper_Name:
                case Constants.Http_Reaper_name:

                    _logger.Information("Reaper triggered at: {time}", DateTime.Now);

                    try
                    {
                        await runReaper();
                    } catch (Exception e) {
                        _logger.Fatal(e.Message);
                    }

                    break;
            }
        }

        /// <summary>
        /// runReaper executes the Reaper for the timer process
        /// It is responsible for gathering the settings and the current subscription
        /// that are required by the reaper. This is so that the reaper does not need to
        /// access the DB directly
        /// </summary>
        private async Task runReaper()
        {
            Response response = new Response();
            List<Models.Setting> settings = new List<Models.Setting>();
            List<Models.Subscription> subscriptions = new List<Models.Subscription>();
            List<Models.LocationTZ> locationTZs = new List<Models.LocationTZ>();

            // get the necessary settings by category
            string[] categories = new string[] { "tags", "slack", "lifecycle"};

            // get the settings the reaper will use
            Dictionary<string, dynamic> criteria = new Dictionary<string, dynamic>();
            criteria.Add("category", categories);

            IModel setting = new Setting(_backend, _logger);
            response = setting.Get(criteria);

            settings = response.GetData();

            // get the subscriptions known to the system
            IModel subscription = new Subscription(_backend, _logger);
            response = subscription.GetByName();

            subscriptions = response.GetData();

            // get the location timezones known to the system
            LocationTZ locationTZ = new LocationTZ(_backend, _logger);        

            // Create a notification delay object for the reaper to work with
            NotificationDelay notificationDelay = new NotificationDelay(_backend, _logger);

            // Create an instance of the reaper and process it
            Reaper reaper = new Reaper(_backend, _logger, notificationDelay);
            await reaper.Process(settings, subscriptions, locationTZ);
        }
    }
}