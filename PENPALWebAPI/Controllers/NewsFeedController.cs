using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using PENPAL.DataStore;
using PENPAL.DataStore.APIModel;
using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Templates;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using PubnubApi;


namespace PENPALWebAPI.Controllers
{

    [RoutePrefix("api/NewsFeed")]
    public class NewsFeedController : ApiController
    {

        /// <summary>
        /// Get Searched User Contact Details
        /// </summary>
        /// <param name="searchuser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetNewsFeeds")]
        public ResponseWrapper<GetNewsFeeds_Result> GetNewsFeeds([FromBody] newsUserIDFeedIDModel UserID)
        {
            ResponseWrapper<GetNewsFeeds_Result> objResponseWrapper = null;
            List<GetNewsFeeds_Result> lstNewsFeed = new List<GetNewsFeeds_Result>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstNewsFeed = ObjUser.GetNewsFeedforPost(UserID.User_feedID);


                if (lstNewsFeed.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<GetNewsFeeds_Result>(lstNewsFeed, false, HttpStatusCode.OK, "Success", true, "news feeds provided", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<GetNewsFeeds_Result>(lstNewsFeed, false, HttpStatusCode.OK, "Success", true, "No news feeds available", null);
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<GetNewsFeeds_Result>(lstNewsFeed, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstNewsFeed = null;
            }


        }

        [HttpPost]
        [Route("AddUpdateFeed")]
        public ResponseWrapper<ResultModel> AddUpdateFeed([FromBody] newsFeedModel feedModel)
        {
            // Return Object
            ResponseWrapper<ResultModel> objResponseWrapper = null;
            // List<InsertUpdateResponseModel> lstInsertUpdateModel = new List<InsertUpdateResponseModel>();
            List<ResultModel> lstinsertUpdateModel = new List<ResultModel>();
            ResultModel insertUpdateModel = new ResultModel();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                lstinsertUpdateModel = ObjCustomer.AddEditNewFeed(feedModel);
                //lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<ResultModel>(lstinsertUpdateModel, false, HttpStatusCode.OK, "Success", true,"News feed added or updated successfully", null);

                
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                insertUpdateModel.IsSuccess = false;
                insertUpdateModel.Message = ex.Message;
                lstinsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<ResultModel>(lstinsertUpdateModel, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("RemoveFeed")]
        public ResponseWrapper<ResultModel> RemoveFeed([FromBody] newsUserIDFeedIDModel feedID)
        {
            // Return Object
            ResponseWrapper<ResultModel> objResponseWrapper = null;
           List<ResultModel> lstDeleteModel = new List<ResultModel>();
            ResultModel deleteModel = new ResultModel();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                lstDeleteModel = ObjCustomer.RemoveNewsFeed(feedID.User_feedID);
                //lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<ResultModel>(lstDeleteModel, false, HttpStatusCode.OK, "Success", true, "News feed removed", null);


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                deleteModel.IsSuccess = false;
                deleteModel.Message = ex.Message;
                lstDeleteModel.Add(deleteModel);
                return objResponseWrapper = new ResponseWrapper<ResultModel>(lstDeleteModel, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }


    }
}