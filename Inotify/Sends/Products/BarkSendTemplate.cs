using Inotify.Common;
using Inotify.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Inotify.Sends.Products
{

    public class BarkMessage
    {
        public BarkMessage(string body) : this(string.Empty, body) { }
        public BarkMessage(string title, string body)
        {
            this.Title = title;
            this.Body = body;
        }

        #region 公共属性
        /// <summary>
        /// 标题,加粗
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 正文
        /// </summary>
        public string Body { get; set; } = string.Empty;
        /// <summary>
        /// 自动保存
        /// </summary>
        public string IsArchive { get; set; } = "1";
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; } = string.Empty;
        /// <summary>
        /// 自动复制
        /// </summary>
        public string AutoMaticallyCopy { get; set; } = "0";
        /// <summary>
        /// 复制文本
        /// </summary>
        public string Copy { get; set; } = string.Empty;
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public BarkMessage SetUrl(string url)
        {
            this.Url = url;
            return this;
        }
        /// <summary>
        /// 设置保存，默认保存
        /// </summary>
        /// <returns></returns>
        public BarkMessage SetArchive()
        {
            IsArchive = "1";
            return this;
        }
        /// <summary>
        /// 设置不保存，默认保存
        /// </summary>
        /// <returns></returns>
        public BarkMessage SetNotArchive()
        {
            IsArchive = "0";
            return this;
        }
        /// <summary>
        /// 设置自动复制，默认不自动复制
        /// </summary>
        /// <returns></returns>
        public BarkMessage SetAutoCopy()
        {
            this.AutoMaticallyCopy = "1";
            return this;
        }
        /// <summary>
        /// 设置不自动复制，默认不自动复制
        /// </summary>
        /// <returns></returns>
        public BarkMessage SetNotAutoCopy()
        {
            this.AutoMaticallyCopy = "1";
            return this;
        }
        /// <summary>
        /// 设置自动拷贝的文本，默认拷贝全文
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public BarkMessage SetCopyText(string text)
        {
            Copy = text;
            return this;
        }
        #endregion
    }

    public class BarkAuth
    {

        [InputTypeAttribte(0, "IsArchive", "自动保存", "1")]
        public string IsArchive { get; set; }

        [InputTypeAttribte(0, "AutoMaticallyCopy", "自动复制", "0")]
        public string AutoMaticallyCopy { get; set; }

        [InputTypeAttribte(0, "DeviceToken", "DeviceToken", "DeviceToken",false)]
        public string DeviceToken { get; set; }

    }

    [SendMethodKey("3B6DE04D-A9EF-4C91-A151-60B7425C5AB2", "Bark(未完成)", Order = -1)]
    public class BarkSendTemplate : SendTemplate<BarkAuth>
    {
        private static readonly string Topic = "me.fin.bark";
        private static readonly string KeyID = "LH4T9V5U4R";
        private static readonly string TeamID = "5U8LBRXG3A";
        private static CngKey SecretKey;
 
        public override BarkAuth Auth { get; set; }

        public override bool SendMessage(SendMessage message)
        {
            var barkMessage = new BarkMessage(message.Title, message.Data)
            {
                IsArchive = Auth.IsArchive,
                AutoMaticallyCopy = Auth.AutoMaticallyCopy
            };
            SendMesssage(barkMessage, Auth.DeviceToken);

            return false;
        }

        private bool SendMesssage(BarkMessage barkMessage, string device_Tokne)
        {
            if (SecretKey == null)
            {
                var authPath = Path.Combine(DBManager.Instance.Inotify_Data, "AuthKey_LH4T9V5U4R_5U8LBRXG3A.p8");
                var privateKeyContent = File.ReadAllText(authPath).Split('\n')[1];
                SecretKey = CngKey.Import(Convert.FromBase64String(privateKeyContent), CngKeyBlobFormat.Pkcs8PrivateBlob);
            }

            if (barkMessage == null)
                return false;

            if (device_Tokne == null)
                return false;

            var expiration = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expirationSeconds = (long)expiration.TotalSeconds;

            var alert = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(barkMessage.Body))
                alert.Add("body", barkMessage.Body);
            if (!string.IsNullOrEmpty(barkMessage.Title))
                alert.Add("title", barkMessage.Title);

            var aps = new Dictionary<string, object>
            {
                { "category", "Bark" },
                { "sound", "1107" },
                { "badge", "0" },
                { "mutable-content", "1" },
                { "alert", alert }
            };

            var payload = new Dictionary<string, object>
            {
                { "aps", aps },
                { "isarchive", barkMessage.IsArchive },
                { "automaticallycopy", barkMessage.AutoMaticallyCopy },
                { "iss", TeamID},
                { "iat", expirationSeconds}
            };

            if (!string.IsNullOrEmpty(barkMessage.Url))
                payload.Add("url", barkMessage.Url);

            if (!string.IsNullOrEmpty(barkMessage.Copy))
                payload.Add("copy", barkMessage.Copy);

            var headers = new
            {
                alg = "ES256",
                kid = KeyID
            };

            var hearderString = JObject.FromObject(headers).ToString();
            var payloadString = JObject.FromObject(payload).ToString();
            var accessToken = SignES256(SecretKey, hearderString, payloadString);
            var data = Encoding.UTF8.GetBytes(payloadString);


            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("https://{0}:{1}/3/device/{2}", "api.development.push.apple.com", 443, device_Tokne))
            };
            requestMessage.Headers.Add("authorization", string.Format("bearer {0}", accessToken));
            requestMessage.Headers.Add("apns-id", Guid.NewGuid().ToString());
            requestMessage.Headers.Add("apns-expiration", "0");
            requestMessage.Headers.Add("apns-priority", "10");
            requestMessage.Headers.Add("apns-topic", Topic);
            requestMessage.Method = HttpMethod.Post;
            requestMessage.Content = new ByteArrayContent(data);

            var task = httpClient.SendAsync(requestMessage);
            task.Wait();
            var responseMessage = task.Result;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {

                return true;
            }

            return true;
        }


        private string SignES256(CngKey secretKey, string header, string payload)
        {
            using ECDsaCng dsa = new ECDsaCng(secretKey)
            {
                HashAlgorithm = CngAlgorithm.Sha256
            };
            var unsignedJwtData = Convert.ToBase64String(Encoding.UTF8.GetBytes(header)) + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
            return unsignedJwtData + "." + Convert.ToBase64String(signature);
        }
    }
}
