using Inotify.ThridOauth.Common;
using Newtonsoft.Json.Linq;



namespace Inotify.ThridOauth.Common
{
    public class AuthorizeResult
    {
        public Code Code { get; set; }

        public string Error { get; set; }

        public JObject Result { get; set; }

        public string Token { get; set; }
    }

    public enum Code
    {
        Success,
        Exception,
        UserInfoErrorMsg,
        AccessTokenErrorMsg
    }
}