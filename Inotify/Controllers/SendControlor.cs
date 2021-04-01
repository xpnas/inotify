using Inotify.Data;
using Inotify.Sends;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Inotify.Controllers
{
    [ApiController]
    [Route("api")]
    public class SendController : BaseControlor
    {
        [HttpGet, Route("send")]
        public JsonResult Send(string token, string title, string? data)
        {
            if (DBManager.Instance.IsSendKey(token))
            {
                var message = new SendMessage()
                {
                    Token = token,
                    Title = title,
                    Data = data,
                };
                if (SendTaskManager.Instance.SendMessage(message))
                    return OK();
            }
            return Fail();
        }
    }
}
