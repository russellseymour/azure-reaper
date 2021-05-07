using Azure.Reaper.Lib.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Azure.Reaper.Lib
{
    public class Program
    {

        // protected CosmosClient _cosmosClient;
        protected ILog _logger;
        protected IBackend _backend;
        protected IConfigurationRoot _config;
        protected Response response;

        public Program(IConfigurationRoot config, ILog logger, IBackend backend) //, CosmosClient cosmosClient)
        {
            // this._cosmosClient = cosmosClient;
            this._logger = logger;
            this._backend = backend;
            this._config = config;

            // Create the database
            this.createDatabase();
        }

        public Program() {
            this.createDatabase();
        }

        /// <summary>
        /// Performs the API request for any method that has does not have a name to parse
        /// This is ususally used for such methods as POST
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResponseMessage ProcessAPIRequest(HttpRequest request, string name = null, bool wildcard = false)
        {

            // return processAPI(request, null, false);
            IModel model = getModel();
            Process.API api = new Process.API(model, _backend, _logger, request, name, wildcard);
            return api.Process();
        }

        public void ProcessQueueMessage(Models.QueueMessage message)  
        {
            _logger.Debug("Processing message from queue");

            Process.Queue queue = new Process.Queue(message, _backend, _logger);
            queue.Process();
        }

        public void ProcessTimer([CallerMemberName] string timerTriggerName = "")
        {
            _logger.Debug("Processing timer event");

            Process.Timer timer = new Process.Timer(_backend, _logger, timerTriggerName);
            timer.Process();
        }

        /// <summary>
        /// Using the values in the configuration and the default values in the app,
        /// create the necessary database using the backend
        /// </summary>
        private void createDatabase()
        {
            // get the name of the database to create
            string dbName = this._config[Constants.Token_DatabaseName];
            // string dbName = Environment.GetEnvironmentVariable(Constants.Token_DatabaseName);
            if (String.IsNullOrEmpty(dbName))
            {
                dbName = Constants.Database_Name_Default;
                this._logger.Warning("Database name not set, using default name: {Default}", dbName);
            }
            this._backend.CreateDatabase(dbName);

        }

        /// <summary>
        /// Get model returns the IModel model to work with
        /// </summary>
        /// <returns></returns>
        private IModel getModel() 
        {
            IModel model = null;
            switch (this)
            {

                case Triggers.Http.LocationTZ locationTZ:

                    model = new Models.LocationTZ(this._backend, this._logger);

                    break;

                case Triggers.Http.Setting setting:

                    model = new Models.Setting(this._backend, this._logger);

                    break;

                case Triggers.Http.Subscription subscription:

                    model = new Models.Subscription(this._backend, this._logger);

                    break;

                default:

                    _logger.Fatal("Model is not known to the system: {model}", this.GetType().Name);

                    break;
            }

            if (model != null)
                _logger.Verbose("Creating model instance: {Model}", model.GetType().Name);

            return model;
        }

/*
        private HttpResponseMessage processAPI(HttpRequest request, string name, bool wildcard)
        {
            response = new Response();

            // create an instance of the model entity to work with from the database
            IModel model = getModel();

            // ensure that the model is valid
            if (model == null)
            {
                string message = "Unable to determine model type";

                // log that this is a fatal error for this request
                _logger.Fatal(message);

                // provide a response to the caller to state what has gone wrong
                response.SetMessage(message);
                response.SetError(true);
                response.SetStatusCode(System.Net.HttpStatusCode.InternalServerError);
                // httpResponse = response.CreateResponse();
            } else {

                // call method to handle the request that has been sent
                handleRequest(request, model, name, wildcard);
            }

            return response.CreateResponse();
        }
    */

/*
        private async void handleRequest(HttpRequest request, IModel model, string name = null, bool wildcard = false)
        {

            _logger.Verbose("Processing request: {Method}", request.Method);
            _logger.Debug("Handling model: {model}", model.GetType().Name);

            // use the request http method to determine what needs to be done
            if (HttpMethods.IsGet(request.Method))
            {

                response = model.Get(name, wildcard);
                
            }
            else if (HttpMethods.IsPost(request.Method))
            {

                // attempt to get the JSON from the body of the request
                string json = await getBody(request.Body);

                // check to see if there is an error in the response, if there is return
                if (response.IsError())
                {
                    _logger.Error(response.GetMessage());
                    return;
                }

                // there are no errors to attempt to upsert the data
                // response = await _backend.Upsert(model, json);
                response = await model.Upsert(json);
            }
            else if (HttpMethods.IsDelete(request.Method))
            {
                response = await model.Delete(name, wildcard);
            }
        }
        */

/*
        /// <summary>
        /// Get the body of the request and ensure it is valid JSON
        /// Return the JSON as a string and update the response object with any errors
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task<string> getBody(Stream body)
        {
            // Intialise variables
            // - configure the json string to return
            string json = null;

            // get the JSON data from the body of the request
            string payload = await new StreamReader(body).ReadToEndAsync();

            // ensure that the body is not null and is valid json
            if (String.IsNullOrWhiteSpace(payload))
            {
                response.SetError("Body of request must not be empty", true, HttpStatusCode.BadRequest);
            }
            else if (!IsValidJson(payload))
            {
                response.SetError("Supplied payload is not valid JSON", true, HttpStatusCode.BadRequest);
            }
            else
            {
                json = payload;
            }            

            return json;
        }
        */

/*
        /// <summary>
        /// Helper function to determine if the string that has been passed to the app
        /// is valid JSON
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private bool IsValidJson(string payload)
        {
            var trimmed = payload.Trim();
            bool result = true;

            if ((trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                (trimmed.StartsWith("[") && trimmed.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(trimmed);
                }
                catch (JsonReaderException)
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }
        */      
    }
}