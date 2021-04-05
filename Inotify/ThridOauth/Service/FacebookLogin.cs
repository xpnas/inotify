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
    public class FacebookLogin : LoginBase, IFacebookLogin
    {
        private static string _authorizeUrl;
        private static readonly string _oauthUrl = "https://graph.facebook.com/v2.8/oauth/access_token";
        private static readonly string _userInfoUrl = "https://graph.facebook.com/me";
        private static readonly string _userInfoUrlParams = "fields=picture{url},name&access_token=";

        public FacebookLogin(IHttpContextAccessor contextAccessor, IOptions<FaceBookCredential> options) : base(
            contextAccessor)
        {
            Credential = options.Value;
            _authorizeUrl = "https://www.facebook.com/v2.8/dialog/oauth?client_id=" + Credential.ClientId +
                            "&scope=email,public_profile&response_type=code&redirect_uri=";
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
                    var errMsg = string.Empty;

                    var token = GetAccessToken(code, ref errMsg);

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        return new AuthorizeResult { Code = Code.UserInfoErrorMsg, Error = errMsg };
                    }

                    var accessToken = token.Value<string>("access_token");

                    var user = UserInfo(accessToken, ref errMsg);

                    return string.IsNullOrEmpty(errMsg)
                        ? new AuthorizeResult { Code = Code.Success, Result = user, Token = accessToken }
                        : new AuthorizeResult { Code = Code.AccessTokenErrorMsg, Error = errMsg, Token = accessToken };
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
                { "client_id", Credential.ClientId },
                { "client_secret", Credential.ClientSecret },
                { "code", code },
                { "redirect_uri", RedirectUri }
            };

            var Params = string.Join("&", data.Select(x => x.Key + "=" + x.Value).ToArray());

            using var client = new HttpClient();
            try
            {
                var response = client.PostAsync(_oauthUrl, new StringContent(Params)).Result;

                var result = response.Content.ReadAsStringAsync().Result;

                return JsonCommon.Deserialize(result);
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
                    var content = _userInfoUrlParams + token;

                    var response = wc.PostAsync(_userInfoUrl, new StringContent(content)).Result;

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