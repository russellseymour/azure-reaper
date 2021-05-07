
namespace Azure.Reaper.Lib
{
    public class Constants
    {

        // Tokens - names of the items in the configuration file
        public const string Token_ConnectionString = "connectionString";
        public const string Token_Backend = "backend";
        public const string Token_Logger = "logger";
        public const string Token_DatabaseName = "databaseName";
        
        // Database types
        public const string Database_Name_Default = "reaper";

        // Backend types
        public const string Backend_CosmosTableApi = "cosmostableapi";
        public const string Backend_Default = Backend_CosmosTableApi;

        // Logger types
        public const string Logger_Serilog = "serilog";
        public const string Logger_Default = Logger_Serilog;

        // Table names
        public const string Table_Name_Settings = "settings";
        public const string Table_Name_LocationTimezones = "locationTimezones";
        public const string Table_Name_Subscriptions = "subscriptions";
        public const string Table_Name_NotificationDelay = "notificationDelay";

        // Storage account for queue
        public const string Queue_Storage_Account = "QueueStorageAccount";
        public const string Queue_Name = "logalertqueue";

        // Timer function names
        public const string Timer_Reaper_Name = "ReaperTimer";
        public const string Http_Reaper_name = "ReaperHttp";
    }
}