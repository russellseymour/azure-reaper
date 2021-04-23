using Azure.Reaper.Lib;
using Azure.Reaper.Lib.Interfaces;
using System;

namespace Azure.Reaper.Lib.Models
{
    public class Setting : Model<Setting>, IModel
    {

        /// <summary>
        /// Name of the setting
        /// </summary>
        /// <value></value>
        public string name { get; set; }

        /// <summary>
        /// Value of this setting
        /// </summary>
        /// <value></value>
        public string value { get; set; }

        /// <summary>
        /// Category, or group, that the setting belongs to
        /// </summary>
        /// <value></value>
        public string category { get; set ;}

        /// <summary>
        /// String representing a more descriptive name for the setting
        /// </summary>
        /// <value></value>
        public string displayName { get; set; }

        /// <summary>
        /// Description of the setting illustrating what it is to be used for
        /// </summary>
        /// <value></value>
        public string description { get; set; }

        public Setting(IBackend backend, ILog logger) : base(backend, logger, Constants.Table_Name_Settings)
        {
        }

        public Setting() : base(Constants.Table_Name_Settings)
        {

        }

        public void SetKeys()
        {
            PartitionKey = tableName;
            RowKey = name;
        }
    }
}