
using Azure.Reaper.Lib.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Reaper.Lib
{
    public class Model<T> : TableEntity
    {
        protected List<T> items;

        [IgnoreProperty]
        [JsonIgnore]
        public string tableName { get; set; }

        protected Interfaces.IBackend _backend;

        protected Interfaces.ILog _logger;

        public Model(Interfaces.IBackend backend, Interfaces.ILog logger, string tableName)
        {
            this._backend = backend;
            this.tableName = tableName;
            this._logger = logger;

            // create table for the model
            this.createTable();

        }

        public Model(string tableName) {
            this.tableName = tableName;
        }

        private void createTable()
        {
            _backend.CreateTable(tableName);
        }

        /// <summary>
        /// Parse the JSON into the model T
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private Response parse(string json)
        {
            // initialise variables
            Response response = new Response();

            _logger.Debug("Parsing JSON into list: {type}", typeof(T).ToString());

            // Deserialise the data
            try
            {
                items = JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (System.Exception ex)
            {
                response.SetError(ex.Message, true, System.Net.HttpStatusCode.BadRequest);
                _logger.Error(response.GetMessage());
            }

            if (!response.IsError())
            {
                _logger.Information("Parsing {Number} {Model} items", items.Count, this.GetType().Name);

                // determine how many items there are, if there are 0 then set an error on the response
                if (items.Count == 0)
                {
                    response.SetError("At least one item is expected", true, System.Net.HttpStatusCode.BadRequest);
                    _logger.Error(response.GetMessage());
                }
                else
                {
                    response.SetMessage(String.Format("Parsing {0} {1} items", items.Count, this.GetType().Name));
                }
            }

            return response;
        }

        public async Task<Response> Upsert(string json)
        {
            // initalise variables
            Response response = new Response();

            // attempt to parse the data
            response = parse(json);

            if (response.IsError())
            {
                return response;
            }

            // iterate around the items in the list 
            // foreach (Interfaces.IModel model in items)
            // {
            //     _logger.Debug("{model} item: {value}", model.GetType().Name, model.name);

            //     // call the backend to add or update the item in the backend
            //     response = await _backend.Upsert(model);
            // }

            response = await Upsert(items);

            return response;
        }

        public async Task<Response> Upsert(List<dynamic> items)
        {
            Response response = new Response();

            // iterate around the items in the list 
            foreach (Interfaces.IModel model in items)
            {
                _logger.Debug("{model} item: {value}", model.GetType().Name, model.name);

                // call the backend to add or update the item in the backend
                response = await _backend.Upsert(model);
            }

            return response;
        }

        public async Task<Response> Upsert(dynamic item)
        {
            Response response = new Response();

            List<T> items = new List<T>();
            items.Add(item);

            response = await Upsert(items);

            return response;
        }

        public Response Get(IDictionary<string, dynamic> criteria)
        {
            Response response = new Response();
            List<T> items = new List<T>();

            // call the backend using the dictionary as the criteria
            switch (this.GetType().Name)
            {
                case "Setting":
                    items = _backend.Get<Models.Setting>(Constants.Table_Name_Settings, criteria);
                    break;
                case "NotificationDelay":
                    items = _backend.Get<Models.NotificationDelay>(Constants.Table_Name_NotificationDelay, criteria);
                    break;
            }            

            response.SetData(items);

            return response;
        }

        public Response GetByName(string name = null, bool wildcard = false)
        {

            dynamic result = null;
            Response response = new Response();
            List<T> items = new List<T>();

            switch (this.GetType().Name)
            {
                case "Setting":
                    items = _backend.Get<Models.Setting>(name, wildcard, Constants.Table_Name_Settings, Constants.Table_Name_Settings);
                    break;
                case "LocationTZ":
                    items = _backend.Get<Models.LocationTZ>(name, wildcard, Constants.Table_Name_LocationTimezones, Constants.Table_Name_LocationTimezones);
                    break;
                case "Subscription":
                    items = _backend.Get<Models.Subscription>(name, wildcard, Constants.Table_Name_Subscriptions, Constants.Table_Name_Subscriptions);
                    break;                                   
            }

            _logger.Debug("Retrieved {total} {model} items", items.Count, this.GetType().Name);

            if (items.Count == 0)
            {
                response.SetHTTPStatus("notfound");
            }
            else if (items.Count == 1 && !String.IsNullOrEmpty(name))
            {
                result = items[0];
            }
            else
            {
                result = items;
            }

            response.SetData(result);

            return response;
        }

        public Response GetByCategory(string category)
        {
            List<T> items = new List<T>();
            Response response = new Response();

            switch (this.GetType().Name)
            {
                case "Setting":
                    items = _backend.GetByCategory<Models.Setting>(category, Constants.Table_Name_Settings, Constants.Table_Name_Settings);
                    break;
            }

            _logger.Debug("Retrieved {total} {model} items", items.Count, this.GetType().Name);

            // set the data on the response and return
            response.SetData(items);
            return response;
        }

        public async Task<Response> Delete(string name, bool wildcard = false)
        {
            // initalise variables
            Response response = new Response();

            switch (this.GetType().Name)
            {
                case "Setting":
                    response = await _backend.Delete<Models.Setting>(name, wildcard, Constants.Table_Name_Settings, Constants.Table_Name_Settings);
                    break;
                case "LocationTZ":
                    response = await _backend.Delete<Models.LocationTZ>(name, wildcard, Constants.Table_Name_Settings, Constants.Table_Name_Settings);
                    break;
                case "Subscription":
                    response = await _backend.Delete<Models.Subscription>(name, wildcard, Constants.Table_Name_Settings, Constants.Table_Name_Settings);
                    break;                    
            }

            return response;
        }
    }
}