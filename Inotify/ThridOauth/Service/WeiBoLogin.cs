using Inotify.ThridOauth.Common;
using Inotify.ThridOauth.Entity;
using Inotify.ThridOauth.IService;
using Inotify.ThridOauth.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;






namespace Inotify.ThridOauth.Service
{
    public sealed class WeiBoLogin : LoginBase, ISinaLogin
    {
        private const string OauthUrl = "https://api.weibo.com/oauth2/access_token?";
        private const string UserInfoUrl = "https://api.weibo.com/2/users/show.json?";
        private const string UserInfoUrlParams = "uid={0}&access_token={1}";
        private readonly string _authorizeUrl;

        public WeiBoLogin(IHttpContextAccessor contextAccessor, IOptions<WeiBoCredential> options) : base(
            contextAccessor)
        {
            Credential = options.Value;
            _authorizeUrl = "https://api.weibo.com/oauth2/authorize?client_id=" + Credential.ClientId +
                            "&response_type=code&redirect_uri=";
        }


        public AuthorizeResult Authorize()
        {
            try
            {
                var code = AuthorizeCode;

                if (string.IsNullOrEmpty(code))
                {
                    HttpContext.Response.Redirect(_authorizeUrl + RedirectUri, true);

                    return null;
                }

                if (!string.IsNullOrEmpty(code))
                {
                    var errorMsg = string.Empty;

                    var token = GetAccessToken(code, ref errorMsg);

                    if (!string.IsNullOrEmpty(errorMsg))
                        return new AuthorizeResult { Code = Code.UserInfoErrorMsg, Error = errorMsg };
                    var accessToken = token.Value<string>("access_token");

                    var uid = token.Value<string>("uid");

                    var user = UserInfo(accessToken, uid, ref errorMsg);

                    return string.IsNullOrEmpty(errorMsg)
                        ? new AuthorizeResult { Code = Code.Success, Result = user, Token = accessToken }
                        : new AuthorizeResult { Code = Code.AccessTokenErrorMsg, Error = errorMsg, Token = accessToken };
                }
            }

            catch (Exception ex)
            {
                return new AuthorizeResult { Code = Code.Exception, Error = ex.Message };
            }

            return null;
        }

        private JObject GetAccessToken(string code, ref string errMsg)
        {
            var data = new SortedDictionary<string, string>
            {
                {"client_id", Credential.ClientId},
                {"client_secret", Credential.ClientSecret},
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", RedirectUri}
            };

            var Params = string.Join("&", data.Select(x => x.Key + "=" + x.Value).ToArray());

            using var wb = new HttpClient();
            try
            {
                var accessTokenUrl = OauthUrl + Params;

                var response = wb.PostAsync(accessTokenUrl, null).Result;

                var result = response.Content.ReadAsStringAsync().Result;

                return JsonCommon.Deserialize(result);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;

                return null;
            }
        }

        private static JObject UserInfo(string token, string uid, ref string errMsg)
        {
            try
            {
                string result;

                using (var wc = new HttpClient())
                {
                    var url = UserInfoUrl + string.Format(UserInfoUrlParams, uid, token);

                    var response = wc.GetAsync(url).Result;

                    result = response.Content.ReadAsStringAsync().Result;
                }

                var user = JsonCommon.Deserialize(result);

                return user;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;

                return null;
            }
        }
    }
}