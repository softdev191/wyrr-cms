using PENPALWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using PENPAL.VM.UserManagement;

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Admin")]
    public class AdminController : ApiController
    {

        /// <summary>
        /// Get All ItemDetails
        /// </summary>
        /// <returns>List of Stores</returns>
        [Route("GetDetails")]
        public HttpResponseMessage GetDetails()
        {
            Result response = new Result();
            try
            {

                List<ItemDetailsModel> itemlist = new List<ItemDetailsModel>();
                ItemDetailsModel objItem = new ItemDetailsModel();
                objItem.ItemID = 2;
                objItem.ItemName = "Fruits";


                ItemDetailsModel objItems = new ItemDetailsModel();
                objItems.ItemID = 3;
                objItems.ItemName = "Cadberry";

                ItemDetailsModel objItem3 = new ItemDetailsModel();
                objItem3.ItemID = 4;
                objItem3.ItemName = "Rasberi";

                itemlist.Add(objItem);
                itemlist.Add(objItems);
                itemlist.Add(objItem3);

                response.status = HttpStatusCode.OK;
                response.data = itemlist;
                response.message = "Item Details";
                return Request.CreateResponse(HttpStatusCode.OK, response);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.InnerException);
            }

        }



    }
}