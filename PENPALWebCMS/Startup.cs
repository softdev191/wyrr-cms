using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using Microsoft.AspNet.Identity;
using PENPALWebAPI.Models;
using System.Web;
using System.Linq;
using Microsoft.Owin.Builder;

[assembly: OwinStartupAttribute(typeof(PENPALWebCMS.Startup))]
namespace PENPALWebCMS
{
    public partial class Startup
    {

        public void Configuration(AppBuilder app)
        {
            ConfigureAuth(app);
        }



    }
}
