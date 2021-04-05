using Inotify.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace Inotify.Controllers
{
    public static class Roles
    {
        static Roles()
        {
            User = "user";
            System = "system";

        }
        public static string User { get; set; }

        public static string System { get; set; }

        public static string SystemOrUser { get; set; }

    }

    public static class Policys
    {

        public const string Users = "users";

        public const string Systems = "systems";

        public const string SystemOrUsers = "SystemOrUsers";

        public const string All = "all";
    }


    public class BaseControlor : ControllerBase
    {
        public string UserName
        {
            get
            {
                var principal = HttpContext.User;
                if (principal != null)
                {
                    return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                }
                return null;
            }
        }

        public string Token
        {
            get
            {
                var principal = HttpContext.User;
                if (principal != null)
                {
                    return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
                }
                return null;
            }
        }

        protected JsonResult OK()
        {
            return Json(new
            {
                code = 200,
                message = "sucess",
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult OK(object obj)
        {
            return Json(new
            {
                code = 200,
                message = "sucess",
                data = obj ?? "",
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult Me(string message)
        {
            return Json(new
            {
                code = 200,
                message = message,
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult Fail()
        {
            return Json(new
            {
                code = 404,
                message = "failed",
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult Fail(int code)
        {
            return Json(new
            {
                code = code,
                message = "failed",
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult Fail(int code, string message)
        {
            return new JsonResult(new
            {
                code = code,
                message = message,
                timestamp = DateTime.Now.ToUnix()
            });
        }

        protected JsonResult Json(object obj)
        {
            return new JsonResult(obj);
        }
    }
}
