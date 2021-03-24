using Inotify.ThridOauth.Entity;



namespace Inotify.ThridOauth.Entity
{
    public class CredentialSetting
    {
        /// <summary>
        ///     AppKey
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///     AppSecret
        /// </summary>
        public string ClientSecret { get; set; }
    }
}