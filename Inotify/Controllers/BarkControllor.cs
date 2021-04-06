using Inotify.Common;
using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Sends;
using Inotify.Sends.Products;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Inotify.Controllers
{

    [ApiController]
    [Route("/")]
    public class BarkControllor : BaseController
    {
        [HttpGet, Route("Ping")]
        public JsonResult Ping()
        {
            return Me("pong");
        }

        [HttpGet, Route("Info")]
        public JsonResult Info()
        {
            var dateTime = System.IO.File.GetLastWriteTime(GetType().Assembly.Location);
            var devices = DBManager.Instance.DBase.Query<SendAuthInfo>().Count();
            return Json(new
            {
                version = "v2.0.1",
                build = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                arch = RuntimeInformation.OSDescription,
                commit = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                devices
            });
        }

        [HttpGet, Route("Healthz")]

        public string Healthz()
        {
            return "ok";
        }

        [HttpGet, Route("Register")]
        public JsonResult Register(string? act, string? key, string? devicetoken, string? device_key)
        {
            return !string.IsNullOrEmpty(device_key) ? Register(device_key) : Register(act, key, devicetoken);
        }

        [HttpPost, Route("Register")]
        public JsonResult Register(string? act, string? device_key, string? device_token)
        {
            if (string.IsNullOrEmpty(act))
            {
                return Fail(400, "request bind failed : act is empty");
            }

            if (string.IsNullOrEmpty(device_token))
            {
                return Fail(400, "request bind failed : device_token is empty");
            }

            var userInfo = DBManager.Instance.DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Token == act);
            if (userInfo == null)
            {
                return Fail(400, "request bind failed : act is not registered");
            }
            else
            {
                BarkAuth barkAuth = null;
                SendAuthInfo barkSendAuthInfo = null;
                var barkTemplateAttribute = typeof(BarkSendTemplate).GetCustomAttributes(typeof(SendMethodKeyAttribute), false).OfType<SendMethodKeyAttribute>().First();

                if (!string.IsNullOrEmpty(device_key))
                {
                    barkSendAuthInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Key == device_key);
                    if (barkSendAuthInfo != null)
                    {
                        barkAuth = JsonConvert.DeserializeObject<BarkAuth>(barkSendAuthInfo.AuthData);
                        barkAuth.DeviceToken = device_token;
                        barkSendAuthInfo.AuthData = JsonConvert.SerializeObject(barkAuth);
                        barkSendAuthInfo.ModifyTime = DateTime.Now;
                        DBManager.Instance.DBase.Update(barkSendAuthInfo);
                    }
                }

                if (barkSendAuthInfo == null)
                {
                    if(string.IsNullOrEmpty(device_key))
                        device_key = Guid.NewGuid().ToString("N").ToUpper();
                    barkAuth = new BarkAuth() { DeviceKey = device_key, DeviceToken = device_token, IsArchive = "1", AutoMaticallyCopy = "1", Sound = "1107" };
                    barkSendAuthInfo = new SendAuthInfo()
                    {
                        Name = barkTemplateAttribute.Name,
                        SendMethodTemplate = barkTemplateAttribute.Key,
                        Key = device_key,
                        AuthData = JsonConvert.SerializeObject(barkAuth),
                        UserId = userInfo.Id,
                        CreateTime = DateTime.Now,
                        ModifyTime = DateTime.Now,
                        Active = true,
                    };
                    DBManager.Instance.DBase.Insert(barkSendAuthInfo);
                }

                return Json(new
                {
                    key = device_key,
                    device_key = device_key,
                    device_token = device_token
                });
            }
        }

        [HttpGet, Route("RegisterCheck")]
        public JsonResult Register(string device_key)
        {
            if (string.IsNullOrEmpty(device_key))
            {
                return Fail(400, "device key is empty");
            }
            if (!DBManager.Instance.DBase.Query<SendAuthInfo>().Any(e => e.Key == device_key))
            {
                return Fail(400, "device not registered");
            }

            return OK();
        }
    }
}
