using Microsoft.Owin.Security.OAuth;
using PENPAL.DataStore.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PENPALWebAPI.Models
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {


        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

           // string HashedPassword = "John";//Helper.GetMD5Hash(context.Password.Replace("\r\n", string.Empty));

           //var res = ObjCustomer.Login(context.UserName.Replace("\r\n", string.Empty), HashedPassword);
           // if (!String.IsNullOrEmpty(Convert.ToString(res)))
           // {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, context.UserName));
                claims.Add(new Claim(ClaimTypes.Sid, context.Password));
               
                var identity = new ClaimsIdentity(claims, context.Options.AuthenticationType);
                var claimsPrincipal = new ClaimsPrincipal(identity);
                Thread.CurrentPrincipal = claimsPrincipal;
                context.Validated(identity);
            //}
            //else
            //{
            //    context.SetError("invalid_grant", "The user name or password is incorrect.");

            //}
        }
    }
}