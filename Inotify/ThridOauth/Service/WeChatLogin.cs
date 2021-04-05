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
    public sealed class WeChatLogin : LoginBase, IWeChatLogin
    {
        private const string OauthUrl = "https://api.weixin.qq.com/sns/oauth2/access_token";

        private const string UserInfoUrl = "https://api.weixin.qq.com/sns/userinfo";

        private const string UserInfoUrlParams = "access_token={0}&openid={1}&lang=zh_CN";

        private readonly string _authorizeUrl;


        public WeChatLogin(IHttpContextAccessor contextAccessor, IOptions<WechatCredential> options) : base(
            contextAccessor)
        {
            Credential = options.Value;
            _authorizeUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + Credential.ClientId +
                            "&redirect_uri={0}&response_type=code&scope=snsapi_userinfo#wechat_redirect";
        }

        public AuthorizeResult Authorize()
        {
            try
            {
                var code = AuthorizeCode;

                if (string.IsNullOrEmpty(code))
                {
                    HttpContext.Response.Redirect(string.Format(_authorizeUrl, RedirectUri), true);

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

                    var accessToken = token.Value<string>("access_token");

                    var uid = token.Value<string>("openid");

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

        private static JObject GetAccessToken(string code, ref string errMsg)
        {
            var data = new SortedDictionary<string, string>
            {
                {"appid", Credential.ClientId}, {"secret", Credential.ClientSecret},
                {"grant_type", "authorization_code"}, {"code", code}
            };

            var Params = string.Join("&", data.Select(x => x.Key + "=" + x.Value).ToArray());

            using var client = new HttpClient();
            try
            {
                var response = client.PostAsync(OauthUrl, new StringContent(Params)).Result;

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
                    var content = new StringContent(string.Format(UserInfoUrlParams, token, uid));

                    var response = wc.PostAsync(UserInfoUrl, content).Result;

                    result = response.Content.ReadAsStringAsync().Result;
                }

                var user = JsonCommon.Deserialize(result);

                user.Add("uid", uid);

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