using PENPAL.DataStore;
using PENPAL.DataStore.APIModel;
using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Event")]
    public class EventController : ApiController
    {

        /// <summary>
        /// Add UserEvent Details
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("AddUserEvent")]
        public ResponseWrapperForAddUpdate<ResponseModelWithID> AddEventDetails([FromBody] EventModel Event)
        {
            ResponseWrapperForAddUpdate<ResponseModelWithID> objResponseWrapper = null;
            try
            {
                ResultModelWithID result = new ResultModelWithID();

                #region AddUserEvent

                EventManagementProvider objEventManagement = new EventManagementProvider();
                result = objEventManagement.AddEventDetails(Event);

                #endregion


                if (result.IsSuccess)
                {
                    List<ResponseModelWithID> lstResponseModel = new List<ResponseModelWithID>();
                    lstResponseModel.Add(new ResponseModelWithID { IsSuccess = result.IsSuccess, RecordID = result.RecordID });

                    return objResponseWrapper = new ResponseWrapperForAddUpdate<ResponseModelWithID>(lstResponseModel, HttpStatusCode.OK, true, "Success", "Event Created Successfully");

                }
                else
                {
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<ResponseModelWithID>(null, HttpStatusCode.OK, false, "Fail", "Error while Event Creating");
                }


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapperForAddUpdate<ResponseModelWithID>(null, HttpStatusCode.InternalServerError, false, "Fail", "Error occured while creating event");

            }
        }

    }
}