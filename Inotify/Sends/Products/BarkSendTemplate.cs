using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Inotify.Sends.Products
{

    public class BarkAuth
    {
        [InputTypeAttribte(1, "Sound", "声音", "1107")]
        public string Sound { get; set; }

        [InputTypeAttribte(2, "IsArchive", "自动保存", "1或0")]
        public string IsArchive { get; set; }

        [InputTypeAttribte(3, "AutoMaticallyCopy", "自动复制", "1或0")]
        public string AutoMaticallyCopy { get; set; }

        [InputTypeAttribte(4, "DeviceKey", "DeviceKey", "DeviceKey", true, true)]
        public string DeviceKey { get; set; }

        [InputTypeAttribte(5, "DeviceToken", "DeviceToken", "DeviceToken", true, true)]
        public string DeviceToken { get; set; }

        [InputTypeAttribte(6, "SendUrl", "SendUrl", "SendUrl", true, true)]
        public string SendUrl { get; set; }


    }

    [SendMethodKey("3B6DE04D-A9EF-4C91-A151-60B7425C5AB2", "Bark", Order = 2999, Waring = "BARK通道勿手动添加，请使用APP添加BARK地址绑定")]
    public class BarkSendTemplate : SendTemplate<BarkAuth>
    {
        private static string KeyID;

        private static string TeamID;

        private static CngKey SecretKey;

        public override bool SendMessage(SendMessage message)
        {
            if (SecretKey == null)
            {
                KeyID = SendCacheStore.GetSystemValue("barkKeyId");
                TeamID = SendCacheStore.GetSystemValue("barkTeamId");
                var privateKey = SendCacheStore.GetSystemValue("barkPrivateKey");
                var privateKeyContent = privateKey.Split('\n')[1];
                var decodeKey = Convert.FromBase64String(privateKeyContent);
                SecretKey = CngKey.Import(decodeKey, CngKeyBlobFormat.Pkcs8PrivateBlob);
            }

            if (Auth.DeviceToken == null)
            {
                return false;
            }

            var payload = CreatePayload(message);
            var accessToken = CreateAccessToken(payload);
            var result = CreatePush(payload, accessToken, Auth.DeviceToken);

            return result;

        }

        private string CreatePayload(SendMessage message)
        {
            var expiration = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expirationSeconds = (long)expiration.TotalSeconds;

            var alert = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(message.Data))
            {
                alert.Add("body", message.Data);
            }

            if (!string.IsNullOrEmpty(message.Title))
            {
                alert.Add("title", message.Title);
            }

            var aps = new Dictionary<string, object>
            {
                { "category", "Bark" },
                { "sound", Auth.Sound },
                { "badge", "0" },
                { "mutable-content", "1" },
                { "alert", alert }
            };

            var payload = new Dictionary<string, object>
            {
                { "aps", aps },
                { "isarchive", Auth.IsArchive },
                { "automaticallycopy", Auth.AutoMaticallyCopy },
                { "iss", TeamID},
                { "iat", expirationSeconds}
            };

            if (!string.IsNullOrEmpty(message.Title))
            {
                payload.Add("copy", message.Title);
            }

            var payloadString = JObject.FromObject(payload).ToString();


            return payloadString;
        }

        private string CreateAccessToken(string payload)
        {
            using ECDsaCng dsa = new ECDsaCng(SecretKey)
            {
                HashAlgorithm = CngAlgorithm.Sha256
            };
            var headers = JObject.FromObject(new
            {
                alg = "ES256",
                kid = KeyID
            }).ToString();

            var unsignedJwtData = Convert.ToBase64String(Encoding.UTF8.GetBytes(headers)) + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
            return unsignedJwtData + "." + Convert.ToBase64String(signature);
        }

        private bool CreatePush(string payload, string accessToken, string device_Tokne)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(payload);
                var request = new HttpRequestMessage
                {
                    Version = new Version(2, 0),
                    RequestUri = new Uri($"https://api.development.push.apple.com:443/3/device/{device_Tokne}")
                };
                request.Headers.Add("authorization", string.Format("bearer {0}", accessToken));
                request.Headers.Add("apns-id", Guid.NewGuid().ToString());
                request.Headers.Add("apns-expiration", "0");
                request.Headers.Add("apns-priority", "10");
                request.Headers.Add("apns-topic", "me.fin.bark");
                request.Method = HttpMethod.Post;
                request.Content = new ByteArrayContent(data);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");

                var task = httpClient.SendAsync(request);
                task.Wait();
                var responseMessage = task.Result;
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var _response_uuid = "";
                    if (responseMessage.Headers.TryGetValues("apns-id", out IEnumerable<string> values))
                    {
                        _response_uuid = values.First();
                        Console.WriteLine($"success: '{_response_uuid}'");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var respoinseBody = responseMessage.Content.ReadAsStringAsync().Result;
                    var responseJson = JObject.Parse(respoinseBody);
                    var reason = responseJson.Value<string>("reason");
                    Console.WriteLine($"failure: '{reason}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"exception: '{ex.Message}'");
            }

            return false;
        }
    }
}
