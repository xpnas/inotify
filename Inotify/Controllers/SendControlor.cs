using Inotify.Data;
using Inotify.Sends;

using Microsoft.AspNetCore.Mvc;

namespace Inotify.Controllers
{
    [ApiController]
    [Route("api")]
    public class SendController : BaseControlor
    {
        [HttpGet, Route("send")]
        public JsonResult Send(string token, string title, string? data, string? key)
        {
            if (DBManager.Instance.IsToken(token, out bool hasActive))
            {
                if (!hasActive)
                {
                    return Fail(400, "you have no tunnel is acitve");
                }

                if (!string.IsNullOrEmpty(key))
                {
                    if (DBManager.Instance.IsSendKey(key, out bool isActive))
                    {
                        if (!isActive)
                        {
                            return Fail(400, $"device:{key} tunnel is not acitve");
                        }
                    }
                    else
                    {
                        return Fail(400, $"device:{key} is not registered");
                    }
                }

                var message = new SendMessage()
                {
                    Token = token,
                    Title = title,
                    Data = data,
                    Key = key,
                };

                if (SendTaskManager.Instance.SendMessage(message))
                {
                    return OK();
                }
            }

            return Fail(400, $"token:{token} is not registered");
        }
    }
}
