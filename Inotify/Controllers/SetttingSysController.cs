using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Data.Models.System;
using Inotify.Sends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Inotify.Controllers
{
    [ApiController]
    [Route("api/settingsys")]
    public class SetttingSysController : BaseController
    {
        [HttpGet, Route("GetGlobal"), Authorize(Policys.Systems)]
        public IActionResult GetGlobal()
        {
            var proxyenable = SendCacheStore.GetSystemValue("proxyenable");
            var githubEnable = SendCacheStore.GetSystemValue("githubEnable");
            return OK(new
            {
                sendthread = SendCacheStore.GetSystemValue("sendthread"),
                proxy = SendCacheStore.GetSystemValue("proxy"),
                proxyenable = proxyenable != "" && bool.Parse(proxyenable),
                administrators = SendCacheStore.GetSystemValue("administrators"),
                githubClientID = SendCacheStore.GetSystemValue("githubClientID"),
                githubClientSecret = SendCacheStore.GetSystemValue("githubClientSecret"),
                githubEnable = githubEnable != "" && bool.Parse(githubEnable),
                barkKeyId = SendCacheStore.GetSystemValue("barkKeyId"),
                barkTeamId = SendCacheStore.GetSystemValue("barkTeamId"),
                barkPrivateKey = SendCacheStore.GetSystemValue("barkPrivateKey"),
            });
        }

        [HttpPost, Route("SetGlobal"), Authorize(Policys.Systems)]
        public IActionResult SetGlobal(
            string? sendthread,
            string? administrators,
            string? proxy,
            string? proxyenable,
            string? githubClientID,
            string? githubClientSecret,
            string? githubEnable,
            string? barkKeyId,
            string? barkTeamId,
            string? barkPrivateKey)
        {
            SendCacheStore.SetSystemValue("sendthread", sendthread);
            SendCacheStore.SetSystemValue("administrators", administrators);
            SendCacheStore.SetSystemValue("proxy", proxy);
            SendCacheStore.SetSystemValue("proxyenable", proxyenable);
            SendCacheStore.SetSystemValue("githubClientID", githubClientID);
            SendCacheStore.SetSystemValue("githubClientSecret", githubClientSecret);
            SendCacheStore.SetSystemValue("githubEnable", githubEnable);
            SendCacheStore.SetSystemValue("barkKeyId", barkKeyId);
            SendCacheStore.SetSystemValue("barkTeamId", barkTeamId);
            SendCacheStore.SetSystemValue("barkPrivateKey", barkPrivateKey);

            SendTaskManager.Instance.Stop();
            SendTaskManager.Instance.Run();

            return OK();
        }

        [HttpGet, Route("GetJWT"), Authorize(Policys.Systems)]
        public IActionResult GetJWT()
        {
            return OK(DBManager.Instance.JWT);
        }

        [HttpPost, Route("SetJWT"), Authorize(Policys.Systems)]
        public IActionResult SetJWT(JwtInfo jwt)
        {
            DBManager.Instance.JWT = jwt;
            StartUpManager.Load().Restart();
            return OK();
        }

        [HttpPost, Route("DeleteUser"), Authorize(Policys.Systems)]
        public IActionResult DeleteUser(string userName)
        {
            var userInfo = DBManager.Instance.GetUser(userName);
            var userId = userInfo.Id;
            if (userInfo != null)
            {
                DBManager.Instance.DBase.Delete(userInfo);
                DBManager.Instance.DBase.DeleteMany<SendAuthInfo>().Where(e => e.UserId == userId);
                return OK();
            }
            return Fail();
        }

        [HttpPost, Route("ActiveUser"), Authorize(Policys.Systems)]
        public IActionResult ActiveUser(string userName, bool active)
        {
            var userInfo = DBManager.Instance.GetUser(userName);
            if (userInfo != null)
            {
                userInfo.Active = active;
                DBManager.Instance.DBase.Update(userInfo, e => e.Active);
                return OK();
            }
            return Fail();
        }

        [HttpGet, Route("GetUsers"), Authorize(Policys.Systems)]
        public IActionResult GetUsers(string? query, int page, int pageSize)
        {
            if (query == null)
            {
                return OK(DBManager.Instance.DBase.Query<SendUserInfo>().ToPage(page, pageSize));
            }
            else
            {
                return OK(DBManager.Instance.DBase.Query<SendUserInfo>().Where(e => e.UserName.Contains(query) || e.Email.Contains(query)).ToPage(page, pageSize));
            }
        }

        [HttpGet, Route("GetSendInfos"), Authorize(Policys.Systems)]
        public IActionResult GetSendInfos(string? start, string? end)
        {
            var templates = SendTaskManager.Instance.GetInputTemeplates();
            var sendInfos = DBManager.Instance.DBase.Fetch<SendInfo>().Where(e=>!string.IsNullOrEmpty( e.TemplateID)).ToList();
            var sendInfoQuerys = sendInfos.Where(e => int.Parse(e.Date) >= int.Parse(start) && int.Parse(e.Date) <= int.Parse(end)).ToList();
            var sendInfoGroups = sendInfoQuerys.GroupBy(e => e.Date).Select(e => new { date = e.Key, count = e.Sum(item => item.Count) }).ToList();
            var sendTypeInfoGroups = sendInfoQuerys.GroupBy(e => e.TemplateID).Select(e => new { date = e.Key, count = e.Sum(item => item.Count) }).ToList();

            var result = new
            {
                dataX = sendInfoGroups.Select(e => e.date).ToList(),
                dataY = sendInfoGroups.Select(e => e.count).ToList(),
                items = sendTypeInfoGroups.Select(e => new { name = templates[e.date].Name, value = e.count })
            };

            return OK(result);
        }
    }
}
