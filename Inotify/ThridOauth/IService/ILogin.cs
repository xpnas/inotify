using Inotify.ThridOauth.Common;




namespace Inotify.ThridOauth.IService
{
    public interface ILogin
    {
        AuthorizeResult Authorize();

        string AuthorizeCode { get; }
    }
}