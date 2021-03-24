using Inotify.ThridOauth.Common;
using Inotify.ThridOauth.IService;




namespace Inotify.ThridOauth.IService
{
    public interface ILogin
    {
        AuthorizeResult Authorize();

        string AuthorizeCode { get; }
    }
}