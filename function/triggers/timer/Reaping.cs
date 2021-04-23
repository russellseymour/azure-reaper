using Azure.Reaper.Lib;
using Azure.Reaper.Lib.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Azure.Reaper.Triggers
{
    class Reaping : Program
    {
        // Create a constructor so that necessary objects are injected
        public Reaping(IConfigurationRoot config, ILog logger, IBackend backend) : base(config, logger, backend)
        {}

        [FunctionName(Constants.Timer_Reaper_Name)]
        public void ReaperTimer(
            [TimerTrigger("0 */10 * * * *")] TimerInfo reaperTimer
        )
        {
            ProcessTimer();
        }
    }
}