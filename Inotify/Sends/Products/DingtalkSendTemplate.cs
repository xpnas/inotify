using Inotify.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Inotify.Sends.Products
{

    public class DingtalkAuth
    {
        [InputTypeAttribte(0, "WebHook", "WebHook", "https://oapi.dingtalk.com/robot/send?access_token=xxxxx")]
        public string WebHook { get; set; }


        [InputTypeAttribte(0, "Secret", "签名校验", "SEC77xxxx")]
        public string Secret { get; set; }
    }


    [SendMethodKey("048297D4-D975-48F6-9A91-8B4EF75805C1", "钉钉群机器人", Order = 21)]
    public class DingtalkSendTemplate : SendTemplate<DingtalkAuth>
    {
        public override bool SendMessage(SendMessage message)
        {
            var bodyObject = new
            {
                markdown = new
                {
                    title = $"{message.Title}",
                    text = $"#### {message.Title}\n{message.Data}",
                },
                msgtype = "markdown",

            };

            var timestamp = DateTime.UtcNow.ToUTC();
            var sign = GetHmac(timestamp, Auth.Secret);
            var url = $"{Auth.WebHook}&timestamp={timestamp}&sign={sign}";
            var webRequest = WebRequest.Create(url);

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bodyObject));

            webRequest.Method = "POST";
            webRequest.ContentType = "application/json;charset=utf-8";
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
                    if (resuleJson.Contains("errcode"))
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
            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return HttpUtility.UrlEncode(Convert.ToBase64String(hashmessage), Encoding.UTF8);

        }
    }
}
