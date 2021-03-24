using Inotify.Sends;
using Inotify.ThridOauth.Entity;
using Inotify.ThridOauth.Service;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http;




namespace Inotify.ThridOauth.Service
{
    public class LoginBase
    {
        private const string Code = "code";

        protected static CredentialSetting Credential;

        protected readonly HttpContext HttpContext;

        protected LoginBase(IHttpContextAccessor contextAccessor)
        {
            HttpContext = contextAccessor.HttpContext;
        }

        public string AuthorizeCode
        {
            get
            {
                var result = HttpContext.Request.Query[Code].ToString();

                return !string.IsNullOrEmpty(result) ? result : string.Empty;
            }
        }

        protected string RedirectUri =>
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.Path.Value}";

        protected HttpClient GetHttpClientProxy()
        {
            if (SendCacheStore.GetSystemValue("proxyenable") == "true")
            {
                var proxyurl = SendCacheStore.GetSystemValue("proxy");
                if (proxyurl != null)
                {
                    WebProxy proxy = new WebProxy
                    {
                        Address = new Uri(proxyurl)
                    };
                    HttpClientHandler handler = new HttpClientHandler
                    {
                        Proxy = proxy
                    };
                    HttpClient httpClient = new HttpClient(handler);
                    return httpClient;
                }
            }
            return new HttpClient();
        }
    }
}