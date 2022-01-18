using DotNetOpenAuth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OAuth2Sample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //tool for reading json
            //http://jsonviewer.stack.hu/
            GetValues();
        }

        /// <summary>
        /// Log in using OAuth2 - then adds token to outgoing httpwebrequest
        /// </summary>
        private void GetValues()
        {
            //Log in and create a client
            AuthorizationServerDescription asd = new AuthorizationServerDescription();
            asd.AuthorizationEndpoint = new Uri("https://accounts.airthings.com/authorize");
            asd.TokenEndpoint = new Uri("https://accounts-api.airthings.com/v1/token");

            var client = new WebServerClient(asd, clientIdentifier: "clientId");//client id
            client.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter("clientSecret");//client secret

            //create the httpwebrequest
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://ext-api.airthings.com/v1/devices/deviceId/latest-samples");//deviceId in url

            //define the scope - check api doc - https://developer.airthings.com/consumer-api-docs/#section/Authentication
            IEnumerable<string> scope = new string[] { "read:device:current_values" };
            IAuthorizationState ia = client.GetClientAccessToken(scope);

            client.AuthorizeRequest(httpWebRequest, ia);//add authorization to the httpwebrequest
           
            //the usual stuff. run the req, parse your json
            try
            {
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = "bolle";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    JObject jObj = JObject.Parse(result);
                    JToken pm1 = jObj.SelectToken("data");
                    int valuepm1 = pm1.Value<int>("pm1");//key name - getting key.value
                }
            }
            catch(Exception ex)
            {

            }
            
        }
    }
}


//{
//    {
//        "data": {
//            "battery": 100,
//      "co2": 808.0,
//      "humidity": 30.0,
//      "pm1": 3.0,
//      "pm25": 6.0,
//      "pressure": 1017.6,
//      "radonShortTermAvg": 23.0,
//      "temp": 22.8,
//      "time": 1642546903,
//      "voc": 483.0,
//      "relayDeviceType": "hub"
//        }
//    }
//}