using System.Net;

namespace Inotify.Sends.Products
{
    public class HttpGetAuth
    {
        [InputTypeAttribte(0, "URL", "请求地址", "https://api.day.app/token/{title}/{data}")]
        public string URL { get; set; }
    }


    [SendMethodKey("ADB11045-F2C8-457E-BF7E-1698AD37ED53", "自定义GET", Order = 4)]
    public class HttpGetTemplate : SendTemplate<HttpGetAuth>
    {

        public override bool SendMessage(SendMessage message)
        {
            var url = Auth.URL.Replace("{title}", message.Title).Replace("{data}", message.Data);
            var webRequest = WebRequest.Create(url);

            if (webRequest != null)
            {
                try
                {
                    webRequest.GetResponse().GetResponseStream();
                    return true;
                }
                catch
                {
                    return false;

                }
            }
            return false;
        }
    }
}
