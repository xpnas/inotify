using Inotify.ThridOauth;
using Inotify.ThridOauth.Entity;
using Inotify.ThridOauth.IService;
using Inotify.ThridOauth.Service;
using Microsoft.Extensions.DependencyInjection;
using System;





namespace Inotify.ThridOauth
{
    public static class ThridPartyLoginExtensions
    {
        public static IServiceCollection AddWeChatLogin(this IServiceCollection services,
            Action<WechatCredential> credential)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Configure(credential);
            services.AddScoped<IWeChatLogin, WeChatLogin>();
            return services;
        }

        public static IServiceCollection AddQqLogin(this IServiceCollection services,
            Action<QQCredential> credential)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Configure(credential);
            services.AddScoped<IQqLogin, QqLogin>();
            return services;
        }

        public static IServiceCollection AddSinaLogin(this IServiceCollection services,
            Action<WeiBoCredential> credential)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Configure(credential);
            services.AddScoped<ISinaLogin, WeiBoLogin>();
            return services;
        }

        public static IServiceCollection AddFackbookLogin(this IServiceCollection services,
            Action<FaceBookCredential> credential)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Configure(credential);
            services.AddScoped<IFacebookLogin, FacebookLogin>();
            return services;
        }

        public static IServiceCollection AddGitHubLogin(this IServiceCollection services,
            Action<GitHubCredential> credential)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Configure(credential);
            services.AddScoped<IGitHubLogin, GitHubLogin>();
            return services;
        }
    }
}