
using Azure.Reaper.Lib;
using Azure.Reaper.Lib.Interfaces;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Azure.Reaper.Lib
{

    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            
            // Read in the configuration for the application
            // This might not be necessary as everything in the host and local.settings.json file
            // is available from Environment variables
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfigurationRoot>(c => config);

            // Create an register the Serilog logger
            // var logger = new LoggerConfiguration()
            //     .MinimumLevel.Debug()
            //     .WriteTo.Console()
            //     .CreateLogger();

            // builder.Services.AddSingleton<Serilog.ILogger>(s => logger);

            // determine the type of logger that is being used
            string loggerType = config[Constants.Token_Logger];
            if (String.IsNullOrEmpty(loggerType))
            {
                loggerType = Constants.Logger_Default;
            }

            ILog logger = null;
            switch (loggerType)
            {
                case Constants.Logger_Serilog:

                    logger = new Loggers.AppSerilog();

                    break;

                default:

                    logger = new Loggers.AppSerilog();
                    break;
            }

            // add the logger for DI
            logger.Information("Logger type: {Logger}", loggerType);
            builder.Services.AddSingleton<ILog>(l => logger);

            // get the connection string to use to connect to the backend
            string connectionString = config[Constants.Token_ConnectionString];
            logger.Verbose("Backend connection string: {ConnectionString}", connectionString);

            // Ensure that the backend has been set, if not set as CosmosTableAPI
            string backendName = config[Constants.Token_Backend];
            if (String.IsNullOrEmpty(backendName))
            {
                backendName = Constants.Backend_Default;
                logger.Warning("Backend type has not been set, using default backend: {Default}", backendName);
            }
            
            logger.Debug("Backend: {Backend}", backendName);
            
            IBackend backend = null;
            switch (backendName.ToLower())
            {
                case Constants.Backend_CosmosTableApi:

                    backend = new Storage.CosmosTableAPI(logger, connectionString);

                    break;

                default:

                    string error = String.Format("A backend type must be specified using the '{0}' value in settings", Constants.Token_Backend); 
                    logger.Error(error);
                    throw new Exception(error);
            }

            // Add the backend as a singleton to the app
            builder.Services.AddSingleton<IBackend>(b => backend);
        }   
    }
}