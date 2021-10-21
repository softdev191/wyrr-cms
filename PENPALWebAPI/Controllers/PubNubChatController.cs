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

    [RoutePrefix("api/Chat")]
    public class PubNubChatController : ApiController
    {

      

        /// <summary>
        /// Get Searched User Contact Details
        /// </summary>
        /// <param name="searchuser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetAllUsersByName")]
        public ResponseWrapper<UserSearchDetails> GetUsersByName(SearchedUserModel searchuser)
        {
            ResponseWrapper<UserSearchDetails> objResponseWrapper = null;
            List<UserSearchDetails> lstUserDetails = new List<UserSearchDetails>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstUserDetails = ObjUser.GetUsersByName(searchuser);


                if (lstUserDetails.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Success", true, "Searched Contact Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Fail", true, "Searched Contact Details", null);
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstUserDetails = null;
            }


        }


        /// <summary>
        ///Send Invite to user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendInvitetoUser")]
        public ResponseWrapper<SendChatInviteToUser_Result> SendInvitetoUser([FromBody] InviteUserModel inviteModel)
        {
            // Return Object
            ResponseWrapper<SendChatInviteToUser_Result> objResponseWrapper = null;
            List<SendChatInviteToUser_Result> lstInsertUpdateModel = new List<SendChatInviteToUser_Result>();
            SendChatInviteToUser_Result insertUpdateModel = new SendChatInviteToUser_Result();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                lstInsertUpdateModel = ObjCustomer.sendInviteToUser(inviteModel);
                //lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<SendChatInviteToUser_Result>(lstInsertUpdateModel, false,HttpStatusCode.OK,  "Success",true, lstInsertUpdateModel[0].response, null);


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                insertUpdateModel.response = ex.Message;
                lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<SendChatInviteToUser_Result>(lstInsertUpdateModel, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }

        /// <summary>
        ///Get Chat Invites List
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetChatInvites")]
        public ResponseWrapper<UserSearchDetails> GetChatInvites([FromBody] ChatUserModel chatUser)
        {
            ResponseWrapper<UserSearchDetails> objResponseWrapper = null;
            List<UserSearchDetails> lstUserDetails = new List<UserSearchDetails>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstUserDetails = ObjUser.GetChatInvitesByUser(chatUser);

               // if (lstUserDetails.Count > 0)
              //  {
                    return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Success", true, "Searched Contact Details", null);
               // }
              //  else
               // {
                //    return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Fail", true, "Searched Contact Details", null);
               // }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstUserDetails = null;
            }
        }

        /// <summary>
        ///Get Recent Chats
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetRecentChats")]
        public ResponseWrapper<UserSearchDetails> GetRecentChats([FromBody] ChatUserModel chatUser)
        {
            ResponseWrapper<UserSearchDetails> objResponseWrapper = null;
            List<UserSearchDetails> lstUserDetails = new List<UserSearchDetails>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstUserDetails = ObjUser.GetRecentChatsByUser(chatUser);
               // SubscribetoChannels(chatUser);
              //  if (lstUserDetails.Count > 0)
              //  {
                    return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Success", true, "Searched Contact Details", null);
              //  }
              //  else
              //  {l
              //      return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.OK, "Fail", true, "Searched Contact Details", null);
              //  }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserSearchDetails>(lstUserDetails, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstUserDetails = null;
            }
        }

        /// <summary>
        ///Send Invite to user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AcceptRejectResendInvite")]
        public ResponseWrapper<AcceptRejectResendChatInvite_Result> AcceptRejectResendInvite([FromBody] ChatInviteStatus chatStatusModel)
        {
            // Return Object
            ResponseWrapper<AcceptRejectResendChatInvite_Result> objResponseWrapper = null;
            List<AcceptRejectResendChatInvite_Result> lstInsertUpdateModel = new List<AcceptRejectResendChatInvite_Result>();
            AcceptRejectResendChatInvite_Result insertUpdateModel = new AcceptRejectResendChatInvite_Result();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                lstInsertUpdateModel = ObjCustomer.AcceptRejectChatInvite(chatStatusModel);
                //lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<AcceptRejectResendChatInvite_Result>(lstInsertUpdateModel, false, HttpStatusCode.OK, "Success", true, lstInsertUpdateModel[0].response, null);


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                insertUpdateModel.response = ex.Message;
                lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<AcceptRejectResendChatInvite_Result>(lstInsertUpdateModel, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
           
        }

        /// <summary>
        ///Send Invite to user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateRecentChat")]
        public ResponseWrapper<UpdateRecentChat_Result> UpdateRecentChat([FromBody] ChatInviteStatus chatStatusModel)
        {
            // Return Object
            ResponseWrapper<UpdateRecentChat_Result> objResponseWrapper = null;
            List<UpdateRecentChat_Result> lstInsertUpdateModel = new List<UpdateRecentChat_Result>();
            UpdateRecentChat_Result insertUpdateModel = new UpdateRecentChat_Result();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                lstInsertUpdateModel = ObjCustomer.UpdateRecentChat(chatStatusModel);
                //lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<UpdateRecentChat_Result>(lstInsertUpdateModel, false, HttpStatusCode.OK, "Success", true, lstInsertUpdateModel[0].response, null);


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                insertUpdateModel.response = ex.Message;
                lstInsertUpdateModel.Add(insertUpdateModel);
                return objResponseWrapper = new ResponseWrapper<UpdateRecentChat_Result>(lstInsertUpdateModel, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
           
        }

        public void SubscribetoChannels(ChatUserModel chatUser)
        {
            PNConfiguration pnConfiguration = new PNConfiguration();
            pnConfiguration.SubscribeKey = "sub-c-e2ffc3cc-5f83-11e9-9fda-dee590861e69";
            pnConfiguration.PublishKey = "pub-c-82e51af5-3e11-4e3f-9e0b-9ebd3d4c51b9";
            pnConfiguration.SecretKey = "sec-c-YzU2Nzg4ZTYtODU3Yi00NTlhLWE4MTctZDRiZmI0N2ViODhh";
            pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
            pnConfiguration.Uuid = "";

            Pubnub pubnub = new Pubnub(pnConfiguration);
            List<UserSearchDetails> lstUserDetails = new List<UserSearchDetails>();
            UserManagementProvider ObjUser = new UserManagementProvider();
            lstUserDetails = ObjUser.GetChatInvitesByUser(chatUser);
            foreach(var item in lstUserDetails)
            {
                var Chn = "WYRR_CHN_" + item.ConnectionID.ToString();
                var UserID = "WYRR_USR_" + chatUser.UserID.ToString();
                pubnub.ChangeUUID(UserID);
                pubnub.Subscribe<string>().Channels(new string[] { Chn }).Execute();

            }
            
        }

    }
}