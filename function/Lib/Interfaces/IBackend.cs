using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Reaper.Lib.Interfaces
{
    public interface IBackend
    {

        /// <summary>
        /// Creates the database in the specified backend
        /// This can be a no-op method
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        bool CreateDatabase(string dbName = null);

        void CreateTable(string tableName);

        Task<Response> Upsert(IModel model);

        dynamic Get<T>(string name, bool wildcard, string partitionKey, string tableName) where T : TableEntity, new();

        dynamic Get<T>(string tableName, IDictionary<string, dynamic> criteria) where T : TableEntity, new();

        dynamic GetByCategory<T>(string category, string partitionKey, string tableName) where T : TableEntity, new();

        Task<Response> Delete<T>(string name, bool wildcard, string partitionKey, string tableName) where T : TableEntity, new();

    }
}