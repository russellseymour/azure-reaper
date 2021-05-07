
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
using Azure.Reaper.Lib.Interfaces;

namespace Azure.Reaper.Triggers.Http
{
    public class Reaper : Program
    {

        private const string ExecuteFunctionName = "ReaperHTTPTrigger";

        private const string MainTag = "reaper";

        public Reaper(IConfigurationRoot config, ILog logger, IBackend backend) : base(config, logger, backend)
        {}

        [FunctionName(Constants.Http_Reaper_name)]
        [OpenApiOperation(operationId: Constants.Http_Reaper_name, tags: new [] { MainTag })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Reaper.Lib.Models.Setting), Description = "Settings from the database")]
        public async Task<HttpResponseMessage> ReaperHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/reaper")] HttpRequest request
        )
        {

            // Call the timer process
            await Task.Run(() => ProcessTimer());

            Lib.Response response = new Lib.Response();
            HttpResponseMessage responseMessage = response.CreateResponse();

            return responseMessage;
        }        
    }
}