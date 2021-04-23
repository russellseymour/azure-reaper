
using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper.Lib.Extensions;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Azure.Reaper.Lib.Storage
{
    public class CosmosTableAPI : IBackend
    {

        // Property to hold the logger for the class
        private ILog _logger;
        private CloudTableClient client;

        /// <summary>
        /// Constructor for the CosmosTableAPI backend.
        /// Accepts a logger so it can output information - this might break the idea of a singleton
        /// logger throughout the application
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        public CosmosTableAPI(ILog logger, string connectionString)
        {
            this._logger = logger;

            // call method to create the tableAccount
            createClient(connectionString);

            // create all the necessary tables
            // createTables();
        }

        /// <summary>
        /// No-op method to the the CosmostTableAPI to return true to state that the database
        /// has been created. Databases are not required in this backend.
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public bool CreateDatabase(string dbName = null)
        {
            this._logger.Debug("Not creating database as it is not required for this backend type");
            return true;
        }

        public async Task<Response> Upsert(IModel model)
        {
            // initialise variables
            Response status = new Response();

            // Create the operation to insert or merge the item in the database
            _logger.Verbose("Creating table operation for upsert: {model}", model.GetType().Name);
            model.SetKeys();
            TableOperation operation = TableOperation.InsertOrMerge((ITableEntity)model);

            // Perform the operation
            CloudTable table = client.GetTableReference(model.tableName);
            TableResult result = await table.ExecuteAsync(operation);

            return status;
        }

        public dynamic Get<T>(string name, bool wildcard, string partitionKey, string tableName) where T : TableEntity, new()
        {
            List<T> items = new List<T>();
            StringBuilder sbQuery = new StringBuilder();

            // call the Get method to get the necessary information
            Dictionary<string, dynamic> criteria = new Dictionary<string, dynamic>();
            criteria.Add("PartitionKey", partitionKey);
            
            if (!String.IsNullOrEmpty(name)) 
            {
                criteria.Add("RowKey", name);
            }

            items = Get<T>(tableName, criteria);

            return items;

        }

        public dynamic Get<T>(string tableName, IDictionary<string, dynamic> criteria) where T : TableEntity, new()
        {
            List<T> items = new List<T>();
            StringBuilder sbQuery = new StringBuilder();

            // iterate over the key value pairs in the dictionary
            foreach (KeyValuePair<string, dynamic> entry in criteria)
            {

                string fragment;

                switch (entry.Value.GetType().ToString())
                {
                    case "System.String":
                        fragment = String.Format("{0} eq '{1}'", entry.Key, entry.Value);
                        sbQuery.AppendDelim(fragment, " and ");
                        break;

                    case "System.String[]":

                        StringBuilder orQuery = new StringBuilder();

                        // iterate around the string array and add to the StringBuilder
                        for (int i = 0; i < entry.Value.Length; i ++)
                        {
                            fragment = String.Format("{0} eq '{1}'", entry.Key, entry.Value[i]);
                            orQuery.AppendDelim(fragment, " or ");
                        }

                        // add the or query to the main sbQuery
                        sbQuery.AppendDelim(orQuery.ToString(), " and ");

                        break;
                }
            }

            // get the query from the query string builder
            string query = sbQuery.ToString();
            _logger.Debug("Query: {query}", query);

            // create a reference to the table name and then create a query
            CloudTable table = client.GetTableReference(tableName);
            TableQuery<T> tblQuery = new TableQuery<T>().Where(query);

            items = table.ExecuteQuery(tblQuery).ToList();

            return items;
        }

        public dynamic GetByCategory<T>(string category, string partitionKey, string tableName) where T : TableEntity, new()
        {
            List<T> items = new List<T>();

            // call the Get method to get the necessary information
            Dictionary<string, dynamic> criteria = new Dictionary<string, dynamic>();
            criteria.Add("PartitionKey", partitionKey);
            criteria.Add("category", category);

            items = Get<T>(tableName, criteria);

            return items;
        }

        public async Task<Response> Delete<T>(string name, bool wildcard, string partitionKey, string tableName) where T : TableEntity, new()
        {
            Response response = new Response();

            // initialise a count
            int count = 0;

            // get a reference to the model table
            CloudTable table = client.GetTableReference(tableName);

            // get a list of the items that need to be deleted
            List<T> items = Get<T>(name, wildcard, partitionKey, tableName);

            // iterate around the items and delete each one
            foreach (var item in items)
            {
                TableOperation operation = TableOperation.Delete(item);
                await table.ExecuteAsync(operation);

                count ++;
            }

            response.SetMessage(String.Format("Deleted {0} items of {1} found", count, items.Count()));
            _logger.Information(response.GetMessage());

            return response;
        }

        /// <summary>
        /// Create all the necessary tables for the application
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void CreateTable(string tableName)
        {
            CloudTable table;

            // create a list of all the tables that need to be created
            // List<string> tables = new List<string>();

            // tables.Add(Constants.Table_Name_Settings);
            // tables.Add(Constants.Table_Name_LocationTimezones);
            // tables.Add(Constants.Table_Name_Subscriptions);

            // iterate around all of the tables and ensure each one exists
            //foreach (string tableName in tables)
            //{
                // Check to see if the table exists
                table = client.GetTableReference(tableName);
                if (table.CreateIfNotExists())
                {
                    _logger.Information("Creating table: {Table}", tableName);
                }
                else
                {
                    _logger.Debug("Table already exists: {Table}", tableName);
                }
            //}
        }

        private void createClient(string connectionString)
        {

            // Create a storage account from the connection string
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(connectionString);

            // Create the table client using the storage account
            _logger.Debug("Creating table client");
            client = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string connectionString)
        {
            CloudStorageAccount storageAccount;

            _logger.Debug("Attempting to create storage account from connection string");

            // attempt to create a storage account from the connection string that has been supplied
            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch
            {
                this._logger.Fatal("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid.");
                throw;
            }

            return storageAccount;
        }
    }
}