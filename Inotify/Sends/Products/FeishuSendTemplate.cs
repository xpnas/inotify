using Inotify.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Inotify.Sends.Products
{
    public class FeishuAuth
    {
        [InputTypeAttribte(0, "WebHook", "WebHook", "https://open.feishu.cn/open-apis/bot/v2/hook/5d7b917e-bfb8-4c7e-ba8c-337xxxx")]
        public string WebHook { get; set; }


        [InputTypeAttribte(0, "Secret", "签名校验", "VcgAbeuZOhTZPSP0zxxxx")]
        public string Secret { get; set; }
    }


    [SendMethodKey("C01A08B4-3A71-452B-9D4B-D8EC7EF1D68F", "飞书群机器人", Order = 22)]
    public class FeishuASendTemplate : SendTemplate<FeishuAuth>
    {
        public override bool SendMessage(SendMessage message)
        {
            var timestamp = DateTime.Now.ToUnix() - 10;
            var sign = GetHmac(timestamp, Auth.Secret);

            var bodyObject = new
            {
                content = new
                {
                    text = $"{message.Title}\n{message.Data}",
                },
                msg_type = "text",
                sign = sign,
                timestamp = timestamp,
            };
            Console.WriteLine(bodyObject);

            var webRequest = WebRequest.Create(Auth.WebHook);
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bodyObject));

            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = 0;

            using (var postStream = webRequest.GetRequestStream())
            {
                var requestStream = webRequest.GetRequestStream();
                webRequest.ContentLength = bytes.Length;
                requestStream.Write(bytes, 0, bytes.Length);
            }

            try
            {
                var response = webRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    var resuleJson = reader.ReadToEnd();
                    if (resuleJson.Contains("code"))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetHmac(long timestamp, string secret)
        {
            var stringToSign = $"{timestamp}\n{secret}";
            using var hmacsha256 = new HMACSHA256Final(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hmacsha256.GetHashFinal());
        }
    }

    public class HMACSHA256Final : HMACSHA256
    {
        public HMACSHA256Final(byte[] bytes) : base(bytes)
        {

        }
        public byte[] GetHashFinal()
        {

            return base.HashFinal();
        }
    }
}
