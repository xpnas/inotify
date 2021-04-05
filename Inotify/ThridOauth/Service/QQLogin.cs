using Inotify.ThridOauth.Common;
using Inotify.ThridOauth.Entity;
using Inotify.ThridOauth.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;






namespace Inotify.ThridOauth.Service
{
    public sealed class QqLogin : LoginBase, IQqLogin
    {
        private const string OauthUrl = "https://graph.qq.com/oauth2.0/token";

        private const string OpenidUrl = "https://graph.qq.com/oauth2.0/me";

        private const string UserInfoUrl = "https://graph.qq.com/user/get_user_info";
        private readonly string _authorizeUrl;
        private readonly string _userInfoUrlParams;


        public QqLogin(IHttpContextAccessor contextAccessor, IOptions<QQCredential> options) : base(
            contextAccessor)
        {
            Credential = options.Value;
            _authorizeUrl = "https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id=" +
                            Credential.ClientId + "&redirect_uri=";
            _userInfoUrlParams =
                "format=json&oauth_consumer_key=" + Credential.ClientId + "&openid={0}&access_token={1}";
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
                    {
                        return new AuthorizeResult { Code = Code.UserInfoErrorMsg, Error = errorMsg };
                    }

                    var accessToken = token["access_token"];

                    var user = UserInfo(accessToken, ref errorMsg);

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

        private Dictionary<string, string> GetAccessToken(string code, ref string errMsg)
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
                var response = wb.PostAsync(OauthUrl, new StringContent(Params)).Result;

                var result = response.Content.ReadAsStringAsync().Result;

                var kvs = result.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                return kvs.Select(v => v.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries))
                    .ToDictionary(kv => kv[0], kv => kv[1]);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;

                return null;
            }
        }

        private JObject UserInfo(string token, ref string errMsg)
        {
            try
            {
                string result;

                using (var wc = new HttpClient())
                {
                    var response = wc.PostAsync(OpenidUrl, new StringContent("access_token=" + token)).Result;
                    result = response.Content.ReadAsStringAsync().Result;
                }

                result = result.Replace("callback(", string.Empty).Replace(");", string.Empty).Trim();

                var openid = JsonCommon.Deserialize(result).Value<string>("openid");

                using (var wc = new HttpClient())
                {
                    var response = wc.PostAsync(UserInfoUrl,
                        new StringContent(string.Format(_userInfoUrlParams, openid, token))).Result;

                    result = response.Content.ReadAsStringAsync().Result;
                }

                var user = JsonCommon.Deserialize(result);

                user.Add("openid", openid);

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