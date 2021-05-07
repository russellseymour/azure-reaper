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
    public class Subscription : Program
    {

        private const string DeleteFunctionName = "SubscriptionDelete";
        private const string GetFunctionName = "SubscriptionGet";
        private const string UpsertFunctionName = "SubscriptionUpsert";
        private const string MainTag = "subscriptions";

        public Subscription(IConfigurationRoot config, ILog logger, IBackend backend) : base(config, logger, backend)
        {}  

        [FunctionName(DeleteFunctionName)]
        [OpenApiOperation(operationId: DeleteFunctionName, tags: new [] { MainTag })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Path, Required = false, Type = typeof(string), Summary = "Name of subscription", Description = "Specify the name of the subscription to delete", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Reaper.Lib.Models.Subscription), Description = "Subscriptions from the database")]
        public async Task<HttpResponseMessage> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/subscription/{name}")] HttpRequest request,
            string name
        )
        {
            return await Task.Run(() => ProcessAPIRequest(request, name));
        }  

        [FunctionName(GetFunctionName)]
        [OpenApiOperation(operationId: GetFunctionName, tags: new [] { MainTag })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Path, Required = false, Type = typeof(string), Summary = "Name of subscription", Description = "Specify the name of the subscription to retrieve", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Reaper.Lib.Models.Subscription), Description = "Subsciptions from the database")]
        public async Task<HttpResponseMessage> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/subscription/{name?}")] HttpRequest request,
            string name
        )
        {

            return await Task.Run(() => ProcessAPIRequest(request, name));
        }

        [FunctionName(UpsertFunctionName)]
        [OpenApiOperation(operationId: UpsertFunctionName, tags: new [] { MainTag })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Lib.Models.Subscription[]), Required = true, Description = "One or more subscriptions to add or update")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Azure.Reaper.Lib.Models.Subscription), Description = "Subscriptions from the database")]
        public async Task<HttpResponseMessage> Upsert(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/subscription")] HttpRequest request
        )
        {
            return await Task.Run(() => ProcessAPIRequest(request));
        }        
    }
}

