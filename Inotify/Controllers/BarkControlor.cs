using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Sends;
using Inotify.Sends.Products;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Inotify.Controllers
{

	public class DeviceInfo
	{
		public string? Token { get; set; }

		public string? Key { get; set; }

		public string? DeviceToken { get; set; }

		public string? Device_key { get; set; }

		public string? Device_token { get; set; }
	}

	[ApiController]
	[Route("/")]
	public class BarkControlor : BaseControlor
	{
		[HttpGet, Route("Ping")]
		public JsonResult Ping(string? token)
		{
			return OK();
		}

		[HttpGet, Route("Info")]
		public JsonResult Info(string? token)
		{
			return Json(new
			{
				version = "v2",
				build = "2021.03.29",
				arch = RuntimeInformation.OSDescription,
				commit = "inotfy",
				devices = RuntimeInformation.OSDescription
			});

		}

		[HttpGet, Route("Healthz")]

		public string Healthz(string? token)
		{
			return "ok";
		}

		[HttpPost, Route("Register")]
		public JsonResult Register(DeviceInfo deviceInfo)
		{
			if (!string.IsNullOrEmpty(deviceInfo.Key))
				deviceInfo.Device_key = deviceInfo.Key;

			if (!string.IsNullOrEmpty(deviceInfo.DeviceToken))
				deviceInfo.Device_token = deviceInfo.DeviceToken;

			if (string.IsNullOrEmpty(deviceInfo.Device_key))
				return Fail(400, "request bind failed : device_key is empty");

			if (string.IsNullOrEmpty(deviceInfo.Device_token))
				return Fail(400, "request bind failed : device_token not empty");

			var userInfo = DBManager.Instance.DBase.Query<SendUserInfo>().FirstOrDefault(e => e.Token == deviceInfo.Token);
			if (userInfo == null)
			{
				return Fail(400, "request bind failed : device not registered");
			}
			else
			{

				var barkAuth = new BarkAuth() { DeviceToken = deviceInfo.Device_token };
				var barkTemplateAttribute = typeof(BarkSendTemplate).GetCustomAttributes(typeof(SendMethodKeyAttribute), false).OfType<SendMethodKeyAttribute>().First();
				var barkSendAuthInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.UserId == userInfo.Id && e.SendMethodTemplate == barkTemplateAttribute.Key);
				if (barkSendAuthInfo == null)
				{
					barkSendAuthInfo = new SendAuthInfo()
					{
						Name = barkTemplateAttribute.Name,
						SendMethodTemplate = barkTemplateAttribute.Key,
						AuthData = JsonConvert.SerializeObject(barkAuth),
						UserId = userInfo.Id,
						CreateTime = DateTime.Now,
						ModifyTime = DateTime.Now
					};

					var sendAuthId = Convert.ToInt32(DBManager.Instance.DBase.Insert<SendAuthInfo>(barkSendAuthInfo));
					userInfo.SendAuthId = sendAuthId;
					DBManager.Instance.DBase.Update(userInfo, e => e.SendAuthId);
				}
				else
				{
					barkSendAuthInfo.AuthData = JsonConvert.SerializeObject(barkAuth);
					barkSendAuthInfo.ModifyTime = DateTime.Now;
					DBManager.Instance.DBase.Update(barkSendAuthInfo);
				}

				return Json(new
				{
					key = deviceInfo.Device_key,
					device_key = deviceInfo.Device_key,
					device_token = deviceInfo.Device_token
				});
			}
		}
	}
}
