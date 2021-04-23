using Azure.Reaper.Lib.Interfaces;
using Serilog;
using System;

namespace Azure.Reaper.Lib.Loggers
{

    public class AppSerilog : ILog
    {

        private Serilog.Core.Logger Log;

        public AppSerilog() 
        {
            Log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Verbose(string message, params object[] propertyValues)
        {
            Log.Verbose(message, propertyValues);
        }

        public void Debug(string message, params object[] propertyValues)
        {
            Log.Debug(message, propertyValues);
        }

        public void Information(string message, params object[] propertyValues)
        {
            Log.Information(message, propertyValues);
        }

        public void Warning(string message, params object[] propertyValues)
        {
            Log.Warning(message, propertyValues);
        }

        public void Error(string message, params object[] propertyValues)
        {
            Log.Error(message, propertyValues);
        }

        public void Fatal(string message, params object[] propertyValues)
        {
            Log.Fatal(message, propertyValues);
        }
    }
}