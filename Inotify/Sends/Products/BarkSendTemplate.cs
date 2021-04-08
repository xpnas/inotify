using CorePush.Apple;
using Newtonsoft.Json;
using System;
using System.Net.Http;

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
        private static ApnSender apnSender;

        public override bool SendMessage(SendMessage message)
        {
            if (apnSender == null)
            {
                var keyID = SendCacheStore.GetSystemValue("barkKeyId");
                var teamID = SendCacheStore.GetSystemValue("barkTeamId");
                var privateKey = SendCacheStore.GetSystemValue("barkPrivateKey");
                var privateKeyContent = privateKey.Split('\n')[1];
                var apnSettings = new ApnSettings()
                {

                    TeamId = teamID,
                    AppBundleIdentifier = "me.fin.bark",
                    P8PrivateKey = privateKeyContent,
                    ServerType = ApnServerType.Production,
                    P8PrivateKeyId = keyID,
                };

                apnSender = new ApnSender(apnSettings, new HttpClient());
            }

            var payload = new AppleNotification(
                Guid.NewGuid(),
                message.Data,
                message.Title)
            {
                IsArchive = Auth.IsArchive,
                AutoMaticallyCopy=Auth.AutoMaticallyCopy,
            };
            payload.Aps.Sound = Auth.Sound;

            var response = apnSender.Send(payload, Auth.DeviceToken);

            if (response.IsSuccess)
                return true;
            return false;
        }
    }

    public class AppleNotification
    {
        public class ApsPayload
        {
            public class Alert
            {
                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("body")]
                public string Body { get; set; }

            }

            [JsonProperty("sound")]
            public string Sound { get; set; }

            [JsonProperty("alert")]
            public Alert AlertBody { get; set; }

            [JsonProperty("apns-push-type")]
            public string PushType { get; set; } = "alert";
        }

        public AppleNotification(Guid id, string message, string title = "")
        {
            Id = id;

            Aps = new ApsPayload
            {
                AlertBody = new ApsPayload.Alert
                {
                    Title = title,
                    Body = message
                }
            };
        }

        [JsonProperty("aps")]
        public ApsPayload Aps { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("isarchive")]
        public string IsArchive { get; set; }

        [JsonProperty("automaticallycopy")]
        public string AutoMaticallyCopy { get; set; }
    }
}
