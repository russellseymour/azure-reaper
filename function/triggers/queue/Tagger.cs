
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Azure.Reaper.Lib;
using Azure.Reaper.Lib.Models;
using Azure.Reaper.Lib.Interfaces;

namespace Azure.Reaper.Triggers.Queue
{
    public class Tagger : Program
    {

        private const string FunctionName = "tagger";

        public Tagger(IConfigurationRoot config, ILog logger, IBackend backend) : base(config, logger, backend)
        {}

        [FunctionName(FunctionName)]
        // [StorageAccount(Constants.Queue_Storage_Account)]
        public async void Run(
            [QueueTrigger(Constants.Queue_Name)] Azure.Reaper.Lib.Models.QueueMessage message
        )
        {
            await Task.Run(() => ProcessQueueMessage(message));
        }
    }
}