using Inotify.ThridOauth.IService;



namespace Inotify.ThridOauth.IService
{
    public interface IGitHubLogin : ILogin
    {
        public string GetOauthUrl();
    }
}