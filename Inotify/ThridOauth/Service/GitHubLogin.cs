using Inotify.Sends;
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
using System.Net;
using System.Net.Http;






namespace Inotify.ThridOauth.Service
{
    public class GitHubLogin : LoginBase, IGitHubLogin
    {
        private static readonly string _oauthUrl = "https://github.com/login/oauth/access_token";
        private static string _userInfoUrl = "https://api.github.com/user";
        private readonly string _authorizeUrl;


        public GitHubLogin(IHttpContextAccessor contextAccessor, IOptions<GitHubCredential> options) : base(
            contextAccessor)
        {
            Credential = new CredentialSetting()
            {
                ClientId = SendCacheStore.GetSystemValue("githubClientID"),
                ClientSecret = SendCacheStore.GetSystemValue("githubClientSecret")
            };

            _authorizeUrl = "https://github.com/login/oauth/authorize?client_id=" + Credential.ClientId;
        }

        public AuthorizeResult Authorize()
        {
            try
            {
                var code = AuthorizeCode;

                if (string.IsNullOrEmpty(code))
                {
                    HttpContext.Response.Redirect(string.Format(_authorizeUrl), true);
                    return new AuthorizeResult { Code = Code.Exception, Error = "code is null " };
                }
                else
                {
                    var errorMsg = string.Empty;

                    var token = GetAccessToken(code, ref errorMsg);

                    if (!string.IsNullOrEmpty(errorMsg))
                        return new AuthorizeResult
                        {
                            Code = Code.UserInfoErrorMsg,
                            Error = errorMsg
                        };
                    var accessToken = token.Value<string>("access_token");

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
        }

        private JObject UserInfo(object accessToken, ref string errorMsg)
        {
            try
            {
                string result;
                _userInfoUrl = string.Format(_userInfoUrl, accessToken);
                using (var wc = GetHttpClientProxy())
                {
                    wc.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
                    wc.DefaultRequestHeaders.Add("Authorization", "token " + accessToken);
                    var response = wc.GetAsync(_userInfoUrl).Result;
                    result = response.Content.ReadAsStringAsync().Result;
                }
                var user = JsonCommon.Deserialize(result);
                return user;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;

                return null;
            }
        }

        private JObject GetAccessToken(string code, ref string errorMsg)
        {
            var data = new SortedDictionary<string, string>
            {
                {"client_id",Credential.ClientId},
                {"client_secret",Credential.ClientSecret},
                {"code",code}
            };
            using var client = GetHttpClientProxy();
            try
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = client.PostAsync(_oauthUrl, new FormUrlEncodedContent(data)).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                if (result.Contains("bad_verification_code"))
                {
                    errorMsg = "bad_verification_code";
                    return null;
                }
                return JsonCommon.Deserialize(result);

            }

            catch (Exception ex)
            {
                errorMsg = ex.Message;

                return null;
            }
        }

        public string MidStrEx(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr, StringComparison.Ordinal);
                if (startindex == -1)
                    return result;
                string tmpstr = sourse[(startindex + startstr.Length)..];
                endindex = tmpstr.IndexOf(endstr, StringComparison.Ordinal);
                if (endindex == -1)
                    return result;
                result = tmpstr.Remove(endindex);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return result;
        }

        public string GetOauthUrl()
        {
            return _authorizeUrl;
        }
    }
}