using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Reaper.Lib.Interfaces
{
    public interface IModel
    {
        string name { get; set; }

        string tableName { get; set; }

        Task<Response> Delete(string name, bool wildcard);

        Response Get(IDictionary<string, dynamic> criteria);
        Response GetByName(string name = null, bool wildcard = false);
        Response GetByCategory(string category);
        Task<Response> Upsert(string json);
        Task<Response> Upsert(List<dynamic> items);
        Task<Response> Upsert(dynamic item);

        

        void SetKeys();
    }
}