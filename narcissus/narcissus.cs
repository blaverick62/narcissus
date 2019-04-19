/*
 * AUTHOR: 2d Lt Braden Laverick
 * ORGANIZATION: 92 COS/DOA
 * PROJECT: Narcissus
 * DESCRIPTION: This project is a proof of concept for a twitter-based remote access trojan. The tool is controlled through
 * direct messages in twitter. The bot requires access to one account to receive DMs.
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace narcissus
{
    public class Target
    {
        public string recipient_id { get; set; }
    }

    public class Entities
    {
        public List<object> hashtags { get; set; }
        public List<object> symbols { get; set; }
        public List<object> user_mentions { get; set; }
        public List<object> urls { get; set; }
    }

    public class MessageData
    {
        public string text { get; set; }
        public Entities entities { get; set; }
    }

    public class MessageCreate
    {
        public Target target { get; set; }
        public string sender_id { get; set; }
        public string source_app_id { get; set; }
        public MessageData message_data { get; set; }
    }

    public class Event
    {
        public string type { get; set; }
        public string id { get; set; }
        public string created_timestamp { get; set; }
        public MessageCreate message_create { get; set; }
    }

    public class __invalid_type__16162985
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Apps
    {
        public __invalid_type__16162985 __invalid_name__16162985 { get; set; }
    }

    public class TwitterResponse
    {
        public List<Event> events { get; set; }
        public Apps apps { get; set; }
    }

    class Narcissus
    {
        static Narcissus narcissusMain = new Narcissus();
        static HttpClient narcissusHTTPClient = new HttpClient();
        static readonly string apiVar = Environment.GetEnvironmentVariable("narcissusKeys", EnvironmentVariableTarget.User);
        static readonly string[] apiKeys = apiVar.Split(';');
        static readonly string oauthKey = apiKeys[0];
        static readonly string oauthToken = apiKeys[2];
        static readonly string oauthKeySecret = apiKeys[1];
        static readonly string oauthTokenSecret = apiKeys[3];

        public async Task<string> GetAsync(string url)
        {            
            using (var response = await narcissusHTTPClient.GetAsync(url))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> PostAsync(string url, HttpContent content)
        {
            using (var response = await narcissusHTTPClient.PostAsync(url, content))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public string Build_AuthHeader(string nonce, string timestamp, string method, string url, Dictionary<string, string> requestParams=null)
        {
            Dictionary<string, string> authorizationParams = new Dictionary<string, string>();
            authorizationParams.Add("oauth_nonce", nonce);
            authorizationParams.Add("oauth_timestamp", timestamp);
            authorizationParams.Add("oauth_consumer_key", oauthKey);
            authorizationParams.Add("oauth_signature_method", "HMAC-SHA1");
            authorizationParams.Add("oauth_token", oauthToken);
            authorizationParams.Add("oauth_version", "1.0");
            if(requestParams != null)
            {
                foreach(KeyValuePair<string,string> paramPair in requestParams)
                {
                    authorizationParams.Add(paramPair.Key, paramPair.Value);
                }
            }

            string signatureBaseString = "";
            List<string> paramKeys = authorizationParams.Keys.ToList();
            paramKeys.Sort();
            foreach(string requestParam in paramKeys)
            {
                if(requestParam == paramKeys.First())
                {
                    signatureBaseString = signatureBaseString + 
                        Uri.EscapeDataString(requestParam) +
                        "=" +
                        Uri.EscapeDataString(authorizationParams[requestParam]);
                } else
                {
                    signatureBaseString = signatureBaseString +
                        "&" +
                        Uri.EscapeDataString(requestParam) +
                        "=" +
                        Uri.EscapeDataString(authorizationParams[requestParam]);
                }  
            }

            string signatureString = String.Format("{0}&{1}&{2}",
                method,
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(signatureBaseString));

            byte[] signingKey = Encoding.UTF8.GetBytes(Uri.EscapeDataString(oauthKeySecret) + 
                "&" + 
                Uri.EscapeDataString(oauthTokenSecret));

            HMACSHA1 oauthSigner = new HMACSHA1(signingKey);
            byte[] oauthHashBytes = oauthSigner.ComputeHash(Encoding.UTF8.GetBytes(signatureString));
            string oauthSignature = Convert.ToBase64String(oauthHashBytes);

            authorizationParams.Add("oauth_signature", oauthSignature);

            string headerString = "";
            List<string> headerKeys = authorizationParams.Keys.ToList();
            headerKeys.Sort();
            foreach(string headerParam in headerKeys)
            {   
                if(headerParam == headerKeys.Last())
                {
                    headerString = headerString +
                    Uri.EscapeDataString(headerParam) +
                    "=" +
                    "\"" +
                    Uri.EscapeDataString(authorizationParams[headerParam]) +
                    "\"";
                } else
                {
                    headerString = headerString +
                    Uri.EscapeDataString(headerParam) +
                    "=" +
                    "\"" +
                    Uri.EscapeDataString(authorizationParams[headerParam]) +
                    "\", ";
                }
            }

            return headerString;
        }

        public string Receive_DM()
        {
            string method = "GET";
            string dmReceiveUrl = "1.1/direct_messages/events/list.json";

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusHTTPClient.BaseAddress.ToString() + dmReceiveUrl);
          
            narcissusHTTPClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            string twitterResponse = narcissusMain.GetAsync(dmReceiveUrl).Result;

            return twitterResponse;
        }

        public string Send_DM(string recipientId)
        {
            string method = "POST";
            string dmSendUrl = "1.1/direct_messages/events/new.json";
            Dictionary<string, string> sendDMParams = new Dictionary<string, string>();
            sendDMParams.Add("type", "message_create");
            sendDMParams.Add("message_create.target.recipient_id", recipientId);
            string message = "Boy I'm really about to GET your pickle chin ass";
            sendDMParams.Add("message_create.message_data", message);

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusHTTPClient.BaseAddress.ToString() + dmSendUrl);

            string postData = "{\"event\": " +
                "{" +
                "\"type\": \"message_create\", " +
                "\"message_create\": {" +
                    "\"target\": {\"recipient_id\": " + "\"" + sendDMParams["message_create.target.recipient_id"] +"\"" + "}," +
                    "\"message_data\": {\"text\": " + "\"" + sendDMParams["message_create.message_data"] +"\"" + "}" +
                    "}" +
                "}" +
                "}";
    
            HttpContent messageContent = new StringContent(postData);

            narcissusHTTPClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            narcissusHTTPClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string twitterResponse = narcissusMain.PostAsync(dmSendUrl, messageContent).Result;

            return twitterResponse;
        }

        public static void MessageListener()
        {
            string twitterResponse;
            JavaScriptSerializer messageParser = new JavaScriptSerializer();
            TwitterResponse responseData;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime latestTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            while (true)
            {
                try
                {
                    twitterResponse = narcissusMain.Receive_DM();
                    responseData = messageParser.Deserialize<TwitterResponse>(twitterResponse);
                    foreach (Event dmEvent in responseData.events)
                    {
                        if (epoch.AddMilliseconds(Convert.ToDouble(dmEvent.created_timestamp)) > latestTime && dmEvent.message_create.sender_id != "35088627")
                        {
                            latestTime = epoch.AddMilliseconds(Convert.ToDouble(dmEvent.created_timestamp));
                            Console.WriteLine(latestTime.ToString());
                            narcissusMain.Send_DM(dmEvent.message_create.sender_id);
                        }
                    }
                    Thread.Sleep(60000);
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("Narcissus listener exited safely");
                    break;
                }
            } 
        }

        static void Main(string[] args)
        {
            narcissusHTTPClient.BaseAddress = new Uri("https://api.twitter.com");
            Thread listenerThread = new Thread(new ThreadStart(MessageListener));
            listenerThread.Start();
            Console.WriteLine("Enter 'q' to exit...");
            string exitChar = "";
            while (exitChar != "q")
            {
                exitChar = Console.ReadLine();
            }
            listenerThread.Abort();
        }
    }
}
