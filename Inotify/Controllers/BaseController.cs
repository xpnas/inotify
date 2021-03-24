using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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


    public class BaseController : ControllerBase
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

        protected JsonResult OK(object? obj = null)
        {
            return new JsonResult(new
            {
                code = 200,
                data = obj ?? "sucess"
            });
        }

        protected JsonResult Fail()
        {
            return new JsonResult(new
            {
                code = 404,
                data = "fail"
            });
        }

        protected JsonResult Fail(int code)
        {
            return new JsonResult(new
            {
                code,
                data = "fail"
            });
        }

        protected JsonResult Fail(int code, string message)
        {
            return new JsonResult(new
            {
                code,
                message
            });
        }



    }
}
