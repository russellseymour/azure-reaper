using System;

using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;

namespace Azure.Reaper.Lib.Config
{
    public class OpenApiOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = "1.0.0",
            Title = "Azure Reaper Configuration API",
            Description = "Configuration API for the Azure Reaper function",
            License = new OpenApiLicense()
            {
                Name = "GPL-3.0",
                Url = new Uri("http://opensource.org/licenses/GPL-3.0"),
            }   
        };
    }
}