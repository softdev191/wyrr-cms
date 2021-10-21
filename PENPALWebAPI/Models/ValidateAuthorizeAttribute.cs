using PENPAL.DataStore.APIModel;
using PENPALWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace PENPALWebAPI.Models
{
    public class ValidateAuthorizeAttribute: AuthorizeAttribute
    {

        /// <summary>
        /// IsAuthorized
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {

                //get current user ClaimsPrincipal
                ClaimsPrincipal currentPrincipal = HttpContext.Current.User as ClaimsPrincipal;

                //check if currentPrincipal contains data
                if (currentPrincipal != null)
                {
                    //return
                    return true;
                }
                else
                {
                    ResponseWrapper<ResponseModel> objResponseWrapper = null;

                    objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.Unauthorized, "Fail", true, "Authorization has been 123 this request", null);


                    //return error with unauthorized access
                    //actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    //{
                    //    //compose message
                    //    ReasonPhrase = "Authorization has been denied for this request"
                    //};
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized,
                       objResponseWrapper);

                    //return
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}