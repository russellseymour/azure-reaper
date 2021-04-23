using Azure.Reaper.Lib.Interfaces;
using Azure.Reaper;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Reaper.Lib.Process
{
    public class API
    {

        private HttpRequest _request;
        private IBackend _backend;
        private ILog _logger;
        private string _name;
        private bool _wildcard = false;
        private Response response;
        private IModel model;

        public API(IModel model, IBackend backend, ILog logger, HttpRequest request, string name, bool wildcard)
        {
            this.model = model;
            _backend = backend;
            _logger = logger;
            _request = request;
            _name = name;
            _wildcard = false;
        }

        public HttpResponseMessage Process()
        {
            response = new Response();

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
                handleRequest(model);
            }

            return response.CreateResponse();
        }

        private async void handleRequest(IModel model)
        {

            _logger.Verbose("Processing request: {Method}", _request.Method);
            _logger.Debug("Handling model: {model}", model.GetType().Name);

            // use the request http method to determine what needs to be done
            if (HttpMethods.IsGet(_request.Method))
            {

                // determine if searching by name or category based on whether the query 'category'
                // has been set
                string queryCategory = _request.Query["category"];
                bool category = queryCategory == null ? false : bool.Parse(queryCategory);
                _logger.Debug("Retrieving items by category: {category}", category.ToString());

                if (category)
                {
                    response = model.GetByCategory(_name);
                }
                else
                {
                    response = model.GetByName(_name, _wildcard);
                }
                
            }
            else if (HttpMethods.IsPost(_request.Method))
            {

                // attempt to get the JSON from the body of the request
                string json = await getBody();

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
            else if (HttpMethods.IsDelete(_request.Method))
            {
                response = await model.Delete(_name, _wildcard);
            }
        }        

        /// <summary>
        /// Get the body of the request and ensure it is valid JSON
        /// Return the JSON as a string and update the response object with any errors
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private async Task<string> getBody()
        {
            // Intialise variables
            // - configure the json string to return
            string json = null;

            // get the JSON data from the body of the request
            string payload = await new StreamReader(_request.Body).ReadToEndAsync();

            // ensure that the body is not null and is valid json
            if (String.IsNullOrWhiteSpace(payload))
            {
                response.SetError("Body of request must not be empty", true, HttpStatusCode.BadRequest);
            }
            else if (!Utilities.IsValidJson(payload))
            {
                response.SetError("Supplied payload is not valid JSON", true, HttpStatusCode.BadRequest);
            }
            else
            {
                json = payload;
            }            

            return json;
        }


       
    }
}