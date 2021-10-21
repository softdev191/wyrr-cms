using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Authentication;

namespace PENPALWebCMS.Filters
{
    public class PermissionsAttribute: ActionFilterAttribute
    {

        public PermissionsAttribute()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;
            var session = filterContext.HttpContext.Session;

            if (session["CurrentUser"] == null)
            {
                if (request.IsAjaxRequest())
                {
                    response.StatusCode = 590;
                }
                else
                {
                    var url = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
                    response.Redirect(url.Action("Login", "Login"));
                }

                filterContext.Result = new EmptyResult();
            }
            

            base.OnActionExecuting(filterContext);
        }

    }
}