using Azure.Reaper.Lib.Interfaces;

namespace Azure.Reaper.Lib.Models
{
    public class LocationTZ : Model<LocationTZ>, IModel
    {
        /// <summary>
        /// Name of the Azure location to map to a Timezone
        /// </summary>
        /// <value></value>
        public string name { get; set; }

        /// <summary>
        /// ID of the timezone to be used for the resources in the specified
        /// location
        /// </summary>
        /// <value></value>
        public string tzId { get; set; }

        public LocationTZ(IBackend backend, ILog logger) : base(backend, logger, Constants.Table_Name_LocationTimezones)
        {
        }

        public LocationTZ() : base(Constants.Table_Name_LocationTimezones)
        {
        }

        public void SetKeys()
        {
            PartitionKey = tableName;
            RowKey = name;
        }
    }
}