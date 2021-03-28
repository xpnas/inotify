using Inotify.Controllers;
using Inotify.Data;
using Inotify.Sends;
using Inotify.ThridOauth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPoco;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Inotify
{
    public class Startup
    {

        public Startup()
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromSeconds(DBManager.Instance.JWT.ClockSkew),
                        ValidAudience = DBManager.Instance.JWT.Audience,
                        ValidIssuer = DBManager.Instance.JWT.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DBManager.Instance.JWT.IssuerSigningKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var payload = JsonConvert.SerializeObject(new { message = "认证失败", code = 403 });
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            context.Response.WriteAsync(payload);
                            return Task.FromResult(1);

                        },
                        OnForbidden = context =>
                         {
                             var payload = JsonConvert.SerializeObject(new { message = "未经授权", code = 405 });
                             context.Response.ContentType = "application/json";
                             context.Response.StatusCode = StatusCodes.Status200OK;
                             context.Response.WriteAsync(payload);
                             return Task.FromResult(1);
                         }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policys.Users, policy => policy.RequireRole(Roles.User).Build());
                options.AddPolicy(Policys.Systems, policy => policy.RequireRole(Roles.System).Build());
                options.AddPolicy(Policys.SystemOrUsers, policy => policy.RequireRole(Roles.User, Roles.System).Build());
                options.AddPolicy(Policys.All, policy => policy.RequireRole(Roles.User, Roles.System).Build());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddGitHubLogin(p =>
            {
                p.ClientId = SendCacheStore.GetSystemValue("githubClientID");
                p.ClientSecret = SendCacheStore.GetSystemValue("githubClientSecret");
            });
            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseStaticFiles();
            app.UseFileServer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var options = new RewriteOptions();
            options.AddRewrite(@"api/(.*).send/(.*)/(.*)", "api/send?token=$1&title=$2&data=$3", true);
            options.AddRewrite(@"api/(.*).send/(.*)", "api/send?token=$1&title=$2", true);

            app.UseRewriter(options);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}