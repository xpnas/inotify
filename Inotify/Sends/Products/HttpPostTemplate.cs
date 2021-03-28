using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inotify.Sends.Products
{
    public class HttpPostAuth
    {
        [InputTypeAttribte(0, "URL", "请求地址", "https://api.day.app/token/{title}/{data}")]
        public string URL { get; set; }

        [InputTypeAttribte(1, "Encoding", "Encoding", "utf-8|ascii|gbk")]
        public string Encoding { get; set; }

        [InputTypeAttribte(2, "Data", "POST参数", @"{""msgid"":""123456"",""title"":""{title}"",""data"":""{data}""}")]
        public string Data { get; set; }

    }


    [SendMethodKey("A3C1E614-717E-4CF1-BA9B-7242717FC037", "自定义POST", Order = 5)]
    public class HttpPostTemplate : SendTemplate<HttpPostAuth>
    {
        public override HttpPostAuth Auth { get; set; }

        public override bool SendMessage(SendMessage message)
        {
            if (Auth.Data == null)
                Auth.Data = "";

            if (string.IsNullOrEmpty(Auth.Encoding))
                Auth.Encoding = "utf-8";

            var url = Auth.URL.Replace("{title}", message.Title).Replace("{data}", message.Data);
            var webRequest = WebRequest.Create(url);

            if (webRequest != null)
            {
                var data = Auth.Data.Replace("{title}", message.Title).Replace("{data}", message.Data);
                var bytes = Encoding.GetEncoding(Auth.Encoding).GetBytes(data);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-urlencoded";
                webRequest.ContentLength = bytes.Length;
                var requestStream = webRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

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
