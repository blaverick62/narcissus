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

        public string Build_AuthHeader(string nonce, string timestamp, string method, string url)
        {

            string sigMethURL = String.Format(
                "{0}&{1}&",
                method,
                Uri.EscapeDataString(url));

            string sigString = String.Format(
                "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&oauth_timestamp={3}&oauth_token={4}&oauth_version=1.0",
                Uri.EscapeDataString(oauthKey), 
                Uri.EscapeDataString(nonce), 
                Uri.EscapeDataString("HMAC-SHA1"),
                Uri.EscapeDataString(timestamp.ToString()), 
                Uri.EscapeDataString(oauthToken));

            string baseString = method + "&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(sigString);

            byte[] signingKey = Encoding.UTF8.GetBytes(Uri.EscapeDataString(oauthKeySecret) + "&" + Uri.EscapeDataString(oauthTokenSecret));

            HMACSHA1 oauthSigner = new HMACSHA1(signingKey);
            byte[] oauthHashBytes = oauthSigner.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            string oauthSignature = Convert.ToBase64String(oauthHashBytes);

            string authorizationHeader = String.Format(
                "oauth_consumer_key=\"{0}\", " +
                "oauth_nonce=\"{1}\", " +
                "oauth_signature=\"{2}\", " +
                "oauth_signature_method=\"HMAC-SHA1\", " +
                "oauth_timestamp=\"{3}\", " +
                "oauth_token=\"{4}\", " +
                "oauth_version=\"1.0\"",
                Uri.EscapeDataString(oauthKey),
                Uri.EscapeDataString(nonce),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(timestamp),
                Uri.EscapeDataString(oauthToken)
                );

            return authorizationHeader;
        }

        public string Receive_DM()
        {
            string method = "GET";
            string dmReceiveUrl = "1.1/direct_messages/events/list.json";
            string contentType = "application/json";
            string host = "api.twitter.com";

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();

            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusHTTPClient.BaseAddress.ToString() + dmReceiveUrl);
          
            narcissusHTTPClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            string twitterResponse = narcissusMain.GetAsync(dmReceiveUrl).Result;

            return twitterResponse;
        }

        static void Main(string[] args)
        { 
            narcissusHTTPClient.BaseAddress = new Uri("https://api.twitter.com");
            string twitterResponse = narcissusMain.Receive_DM();
            Console.WriteLine(twitterResponse);
        }
    }
}
