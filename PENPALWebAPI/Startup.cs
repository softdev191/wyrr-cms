using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using PENPALWebAPI.Models;
using Microsoft.Owin.Security.OAuth;
using System.Web.Http;
using System.Configuration;
using Microsoft.Owin.Builder;

[assembly: OwinStartup(typeof(PENPALWebAPI.Startup))]

namespace PENPALWebAPI
{
    public partial class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            WebApiConfig.Register(config);
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);

        }
        public void ConfigureOAuth(IAppBuilder app)
        {
            int expiresIn = 1;
            string expTime = ConfigurationManager.AppSettings["TokenExpiryTimeInMinutes"].ToString();
            if (!string.IsNullOrEmpty(expTime))
            {
                expiresIn = Convert.ToInt32(expTime);
            }
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(expiresIn),
                Provider = new AuthorizationServerProvider()
            };
            //Token Generation
            //oAuthServerOptions.AccessTokenFormat = oAuthServerOptions.AccessTokenFormat;
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
        }


    }
}
