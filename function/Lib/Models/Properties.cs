using Newtonsoft.Json;

namespace Azure.Reaper.Lib.Models
{
    public class Properties
    {

        public string responseBody;
        public ResponseBody _responseBody;

        public Properties() {
            this.ReadResponseBody();
        }

        public void ReadResponseBody() {
            // remove the encoding from the claims string
            string json = Azure.Reaper.Utilities.RemoveEncoding(this.responseBody);
            this._responseBody = JsonConvert.DeserializeObject<ResponseBody>(json);
        }  
    }
}