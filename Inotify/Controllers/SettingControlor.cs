using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Sends;
using Inotify.Sends.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inotify.Controllers
{
    [ApiController]
    [Route("api/setting")]
    public class SettingControlor : BaseControlor
    {
        [HttpGet, Authorize(Policys.SystemOrUsers)]
        public JsonResult Index()
        {
            return OK();
        }

        [HttpGet, Route("GetSendTemplates"), Authorize(Policys.SystemOrUsers)]
        public JsonResult GetSendTemplates()
        {
            var sendTemplates = SendTaskManager.Instance.GetInputTemeplates().Values;
            return OK(sendTemplates);
        }

        [HttpGet, Route("GetSendAuths"), Authorize(Policys.SystemOrUsers)]
        public JsonResult GetSendAuths()
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                var barkTemplateAttribute = typeof(BarkSendTemplate).GetCustomAttributes(typeof(SendMethodKeyAttribute), false).OfType<SendMethodKeyAttribute>().First();
                var sendAuthInfos = DBManager.Instance.DBase.Query<SendAuthInfo>().Where(e => e.UserId == userInfo.Id).ToArray();
                var userSendTemplates = new List<InputTemeplate>();
                foreach (var sendAuthInfo in sendAuthInfos)
                {
                    var sendTemplate = SendTaskManager.Instance.GetInputTemplate(sendAuthInfo.SendMethodTemplate);
                    if (sendTemplate != null)
                    {
                       
                        sendTemplate.Key = sendAuthInfo.Key;
                        sendTemplate.SendAuthId = sendAuthInfo.Id;
                        sendTemplate.Name = sendAuthInfo.Name;
                        sendTemplate.AuthData = sendAuthInfo.AuthData;
                        sendTemplate.IsActive = sendAuthInfo.Active;
                        sendTemplate.AuthToTemplate(sendAuthInfo.AuthData);
                        userSendTemplates.Add(sendTemplate);
                    }
                    if (barkTemplateAttribute.Key == sendTemplate.Type)
                    {
                        sendTemplate.Values.FirstOrDefault(e => e.Name == nameof(BarkSendTemplate.Auth.SendUrl)).Value = "";
                    }
                }

                return OK(userSendTemplates);
            }
            return Fail();
        }

        [HttpPost, Route("ActiveSendAuth"), Authorize(Policys.SystemOrUsers)]
        public JsonResult ActiveSendAuth(int sendAuthId, bool state)
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                var authInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Id == sendAuthId && e.UserId == userInfo.Id);
                if (authInfo != null)
                {
                    authInfo.Active = state;
                    DBManager.Instance.DBase.Update(authInfo);
                    return OK(authInfo);
                }
            }
            return Fail();
        }

        [HttpPost, Route("DeleteSendAuth"), Authorize(Policys.SystemOrUsers)]
        public JsonResult DeleteSendAuth(int sendAuthId)
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                var authInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Id == sendAuthId && e.UserId == userInfo.Id);
                if (authInfo != null)
                {
                    DBManager.Instance.DBase.Delete(authInfo);
                    return OK();
                }
            }
            return Fail();
        }

        [HttpPost, Route("AddSendAuth"), Authorize(Policys.SystemOrUsers)]
        public JsonResult AddSendAuth(InputTemeplate inputTemeplate)
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null && inputTemeplate.Type != null && inputTemeplate.Name != null)
            {
                var barkKey = typeof(BarkSendTemplate).GetCustomAttributes(typeof(SendMethodKeyAttribute), false).OfType<SendMethodKeyAttribute>().First().Key;
                if (barkKey == inputTemeplate.Type)
                {
                    return Fail(406, "BARK通道勿手动添加，请使用APP添加BARK地址绑定");
                }
                else
                {
                    var authInfo = inputTemeplate.TemplateToAuth();
                    var sendAuth = new SendAuthInfo()
                    {
                        UserId = userInfo.Id,
                        SendMethodTemplate = inputTemeplate.Type,
                        AuthData = authInfo,
                        Name = inputTemeplate.Name,
                        Key = Guid.NewGuid().ToString("N").ToUpper(),
                        CreateTime = DateTime.Now,
                        ModifyTime = DateTime.Now,
                    };
                    DBManager.Instance.DBase.Insert(sendAuth);
                    return OK(sendAuth);

                }
            }
            return Fail();
        }

        [HttpPost, Route("ModifySendAuth"), Authorize(Policys.SystemOrUsers)]
        public JsonResult ModifySendAuth(InputTemeplate inputTemeplate)
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                var barkTemplateAttribute = typeof(BarkSendTemplate).GetCustomAttributes(typeof(SendMethodKeyAttribute), false).OfType<SendMethodKeyAttribute>().First();
                var oldSendInfo = DBManager.Instance.DBase.Query<SendAuthInfo>().FirstOrDefault(e => e.Id == inputTemeplate.SendAuthId);
                if (oldSendInfo != null && inputTemeplate.Name != null)
                {
                    oldSendInfo.Name = inputTemeplate.Name;
                    oldSendInfo.AuthData = inputTemeplate.TemplateToAuth();
                    oldSendInfo.ModifyTime = DateTime.Now;
                    DBManager.Instance.DBase.Update(oldSendInfo);
                }
                return OK(oldSendInfo);
            }
            return Fail();
        }

        [HttpGet, Route("GetSendKey"), Authorize(Policys.SystemOrUsers)]
        public JsonResult GetSendKey()
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                return OK(userInfo.Token);
            }

            return Fail();

        }

        [HttpGet, Route("ReSendKey"), Authorize(Policys.SystemOrUsers)]
        public JsonResult ReSendKey()
        {
            var userInfo = DBManager.Instance.GetUser(UserName);
            if (userInfo != null)
            {
                userInfo.Token = Guid.NewGuid().ToString("N").ToUpper();
                DBManager.Instance.DBase.Update(userInfo);
                return OK(userInfo.Token);
            }
            return Fail();
        }
    }
}
