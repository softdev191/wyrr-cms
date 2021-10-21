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


namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        #region User Registration
        /// <summary>
        /// Register User Details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RegisterUser")]
        public ResponseWrapperForAddUpdate<UserRegistrationAPIModel> RegisterUser([FromBody] UserModel User)
        {
            // Return Object
            ResponseWrapperForAddUpdate<UserRegistrationAPIModel> objResponseWrapper = null;
            List<UserRegistrationAPIModel> lstRegistration = new List<UserRegistrationAPIModel>();
            try
            {
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];

                #region Validation
                if (User == null || User.Password == string.Empty || User.UserName == string.Empty )
                {
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(null, HttpStatusCode.NotFound, false, "Mandatory Information Is Missing", null);
                }


                #endregion

                #region UserRegistration

                UserManagementProvider ObjUser = new UserManagementProvider();

                UserRegistrationAPIModel checkResult = ObjUser.RegisterUser(User);

                #endregion


                if (checkResult.IsSuccess && checkResult.PublicKey!=string.Empty)
                {
                    if(checkResult.PublicKey!=string.Empty && checkResult.PublicKey!="" && checkResult.PublicKey!=null && checkResult.PublicKey!="NULL" )
                    {
                        lstRegistration.Clear();
                        lstRegistration.Add(new UserRegistrationAPIModel { IsSuccess = checkResult.IsSuccess, RecordID = checkResult.RecordID, OTP = checkResult.OTP, ProfileImageUrl = checkResult.ProfileImageUrl });

                        if(User.PhoneNumber.Length>0)
                        { 
                         #region SMSGateway

                        if (checkResult.OTP != string.Empty)
                        {
                            AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                            PublishRequest publishRequest = new PublishRequest();
                            publishRequest.Message = String.Format(SMSTemplates.otpSMS, checkResult.OTP);
                            //publishRequest.PhoneNumber = "+91" + User.PhoneNumber;
                          
                            publishRequest.PhoneNumber = "+" + User.CountryCode + User.PhoneNumber;
                            PublishResponse results = smsClient.Publish(publishRequest);
                        }


                            #endregion
                        }
                        // Email sending for register user 
                        mstUser objUser = new mstUser();
                        using (var context = new WYRREntities())
                        {
                            objUser = context.mstUsers.Where(x => x.UserEmail.ToLower().Trim() == User.EmailAddress.ToLower().Trim()).FirstOrDefault();
                        }
                        //Changed by Namritha 18-01-2019 Replaced string.Empty which is TrnAmount with OTP

                        if (User.EmailAddress!=string.Empty)
                        {
                            // ResultModel result = ObjUser.SendEmail(objUser, Common.EmailType.RegisterUser, string.Empty);
                            ResultModel result = ObjUser.SendEmail(objUser, Common.EmailType.RegisterUser, checkResult.OTP);
                            if (result.IsSuccess)
                                return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(lstRegistration, HttpStatusCode.OK, true, "Success", "User Registered Successfully");
                            else
                            {
                                lstRegistration.Clear();
                                lstRegistration.Add(new UserRegistrationAPIModel { IsSuccess = checkResult.IsSuccess, RecordID = checkResult.RecordID, OTP = checkResult.OTP, ProfileImageUrl = null });
                                return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(lstRegistration, HttpStatusCode.OK, true, "Fail", "Error occured while sending email.");
                            }
                        }
                        return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(lstRegistration, HttpStatusCode.OK, true, "Success", "User Registered Successfully");


                    }
                   else
                    {
                        lstRegistration.Clear();
                        lstRegistration.Add(new UserRegistrationAPIModel { IsSuccess = checkResult.IsSuccess, RecordID = checkResult.RecordID, OTP = checkResult.OTP, ProfileImageUrl = null });
                        return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(lstRegistration, HttpStatusCode.OK, true, "Fail", "Error occured while creating account, Please try again later.");
                    }
                }
                else
                {
                    lstRegistration.Clear();
                    lstRegistration.Add(new UserRegistrationAPIModel { IsSuccess = checkResult.IsSuccess, RecordID = checkResult.RecordID, OTP = checkResult.OTP, ProfileImageUrl = null });
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(lstRegistration, HttpStatusCode.OK, true, "Fail", checkResult.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapperForAddUpdate<UserRegistrationAPIModel>(null, HttpStatusCode.InternalServerError, false, "Fail", ex.Message);
            }
            finally
            {
                objResponseWrapper = null;
                lstRegistration = null;
            }
        }


        #endregion

        /// <summary>
        /// Validate User Details
        /// </summary>
        /// <param name="phonenumber,password"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ValidateUser")]
        public async Task<ResponseWrapper<UserValidationAPIModel>> Login(UserLoginModel userLogin)
        {
            // Return Object
            ResponseWrapper<UserValidationAPIModel> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();

                List<UserValidationAPIModel> lstUserValidation = new List<UserValidationAPIModel>();

                UserValidationAPIModel result = new UserValidationAPIModel();

                #region ValidateUser
                result = ObjCustomer.Login(userLogin.Phonenumber, userLogin.Password,userLogin.isEmail);
                #endregion
                Logger.Log(result, Logger.LogType.Error);

                if (result.IsSuccess)
                {
                    #region TokenGenrationCode
                    //instantiate HttpClient
                    using (var client = new HttpClient())
                    {
                        //get object for token
                        TokenResponseAPIModel tokenAPIModel = new TokenResponseAPIModel();

                        string apiBaseUri = ConfigurationManager.AppSettings["PenpalAPIBaseUrl"].ToString();

                        //setup client
                        client.BaseAddress = new Uri(apiBaseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //setup login data
                        var formContent = new FormUrlEncodedContent(new[]
                        {
                                                new KeyValuePair<string, string>("grant_type", "password"),
                                                new KeyValuePair<string, string>("username", userLogin.Phonenumber),
                                                new KeyValuePair<string, string>("password", userLogin.Password),
                        });

                        //send request
                        HttpResponseMessage responseMessage = await client.PostAsync(apiBaseUri + "/Token", formContent);

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            //get access token from response body
                            var responseJson = await responseMessage.Content.ReadAsStringAsync();


                            //replace for object
                            responseJson = responseJson.Replace(".issued", "issued").Replace(".expires", "expires");

                            //get object for token
                            tokenAPIModel = JsonConvert.DeserializeObject<TokenResponseAPIModel>(responseJson);

                            result.Token = tokenAPIModel.access_token;
                        }
                        else
                        {
                            result.Token = null;
                        }


                    }

                    #endregion


                    lstUserValidation.Add(new UserValidationAPIModel { IsSuccess = true, UserID = result.UserID, UserName = result.UserName, Token = result.Token, PublicKey = result.PublicKey, SecretKey = result.SecretKey, PhoneNumber = result.PhoneNumber, EmailID = result.EmailID, ProfileImageUrl = result.ProfileImageUrl, Currency = result.Currency });
                    return objResponseWrapper = new ResponseWrapper<UserValidationAPIModel>(lstUserValidation, false, HttpStatusCode.OK, "Success", true, "Valid User", null);
                }
                else
                {
                    lstUserValidation.Add(new UserValidationAPIModel { IsSuccess = false, UserID = 0, UserName = null, Token = null, PublicKey = null, SecretKey = null, PhoneNumber = null });
                    return objResponseWrapper = new ResponseWrapper<UserValidationAPIModel>(lstUserValidation, false, HttpStatusCode.OK, "Fail", true, "Invalid User", null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<UserValidationAPIModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }

        /// <summary>
        /// Forgot Password
        /// </summary>
        /// <param name="EmailID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ForgotPassword")]
        public ResponseWrapper<ResponseModel> UserForgotPassword(UserEmailModel user)
        {
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();
                if (string.IsNullOrEmpty(user.EmailId))
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.NotFound, "Please Enter EmailId", false, "Mandatory information missing.", null);

                mstUser objUser = new mstUser();
                using (var context = new WYRREntities())
                {
                    objUser = context.mstUsers.Where(x => x.UserEmail.ToLower().Trim() == user.EmailId.ToLower().Trim()).FirstOrDefault();
                }

                if (objUser != null)
                {
                    #region ValidUser

                    ResultModel result = ObjUser.SendEmail(objUser, Common.EmailType.ForgotPassword, string.Empty);

                    if (result.IsSuccess)
                    {
                        List<ResponseModel> lstResponseModel = new List<ResponseModel>();
                        lstResponseModel.Add(new ResponseModel { IsSuccess = result.IsSuccess, Message = result.Message });

                        return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, "Email sent successfully", null);
                    }
                    else
                        return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.NotFound, "Error occured while sending mail", false, result.Message, null);

                    #endregion

                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.NotFound, "Fail", false, "Please enter registered Email ID", null);
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Error occured while sending mail", false, ex.Message, null);
            }




        }

        /// <summary>
        /// Get User Contact Details
        /// </summary>
        /// <param name="ContactList"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAllContact")]
        public ResponseWrapper<UserContactDetails> UserContactDetails(UserContactModel user)
        {
            ResponseWrapper<UserContactDetails> objResponseWrapper = null;
            List<UserContactDetails> lstContactDetails = new List<UserContactDetails>();
            try
            {
                #region GetAllContact

                UserManagementProvider ObjUser = new UserManagementProvider();
                bool isNextPagePresent = false;
                lstContactDetails = ObjUser.GetAllContactDetails(user);
                lstContactDetails = lstContactDetails.OrderBy(x => x.UserName).ToList();
                #endregion

                if (lstContactDetails.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(lstContactDetails, isNextPagePresent, HttpStatusCode.OK, "Success", true, "Contact Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.OK, "Fail", true, "", null);
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);

            }
            finally
            {
                objResponseWrapper = null;
                lstContactDetails = null;
            }

        }
        

        /// <summary>
        /// Get User Contact Details
        /// </summary>
        /// <param name="ContactList"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetAllContact_V2")]
        public ResponseWrapper<UserContactDetails> UserContactDetails_V2(UserContactModel_V2 user)
        {
            ResponseWrapper<UserContactDetails> objResponseWrapper = null;
            List<UserContactDetails> lstContactDetails = new List<UserContactDetails>();
            try
            {
                #region GetAllContact

                UserManagementProvider ObjUser = new UserManagementProvider();
                bool isNextPagePresent = false;
                lstContactDetails = ObjUser.GetAllContactDetails_V2(user);
                if(!user.IsFrequentChat)
                {
                    lstContactDetails = lstContactDetails.OrderBy(x => x.UserName).ToList();
                }
             
                #endregion

                if (lstContactDetails.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(lstContactDetails, isNextPagePresent, HttpStatusCode.OK, "Success", true, "Contact Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.OK, "Fail", true, "No data found", null);
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);

            }
            finally
            {
                objResponseWrapper = null;
                lstContactDetails = null;
            }

        }
        

        /// <summary>
        /// Get Searched User Contact Details
        /// </summary>
        /// <param name="searchuser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSearchedContact")]
        public ResponseWrapper<UserContactDetails> GetSearchedContact(SearchedUserContactModel searchuser)
        {
            ResponseWrapper<UserContactDetails> objResponseWrapper = null;
            List<UserContactDetails> lstContactDetails = new List<UserContactDetails>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstContactDetails = ObjUser.GetSearchedContatcDetails(searchuser);

                if (lstContactDetails.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(lstContactDetails, false, HttpStatusCode.OK, "Success", true, "Searched Contact Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.OK, "Fail", true, "Searched Contact Details", null);
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserContactDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstContactDetails = null;
            }


        }


        /// <summary>
        /// Get User Profile
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserProfile")]
        public ResponseWrapper<UserProfileDetails> GetUserProfile(UserOTPModel user)
        {
            // Return Object
            ResponseWrapper<UserProfileDetails> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();

                List<UserProfileDetails> lstUserProfileDetails = new List<UserProfileDetails>();

                UserProfileDetails result = new UserProfileDetails();

                #region GetUserProfileDetails
                result = ObjCustomer.GetUserProfile(user.UserId);

                #endregion


                if (result != null)
                {
                    lstUserProfileDetails.Add(new UserProfileDetails { UserID = result.UserID, UserName = result.UserName, DefaultCurrency = result.DefaultCurrency, EmailAddress = result.EmailAddress, PhoneNumber = result.PhoneNumber, ProfileImageUrl = result.ProfileImageUrl,Publickey=result.Publickey,Secretkey=result.Secretkey });
                    return objResponseWrapper = new ResponseWrapper<UserProfileDetails>(lstUserProfileDetails, false, HttpStatusCode.OK, "Success", true, "User Profile", null);
                }
                else
                {
                    lstUserProfileDetails.Add(new UserProfileDetails { UserID = 0, UserName = null, DefaultCurrency = null, EmailAddress = null, ProfileImageUrl = null, PhoneNumber = null });
                    return objResponseWrapper = new ResponseWrapper<UserProfileDetails>(lstUserProfileDetails, false, HttpStatusCode.OK, "Fail", true, "Invalid User", null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<UserProfileDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }

        /// <summary>
        /// Update User Profile
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateUserDetails")]
        public ResponseWrapperForAddUpdate<UserContactDetails> AddUpdateUserDetails([FromBody] UpdateUserModel user)
        {
            // Return Object
            ResponseWrapperForAddUpdate<UserContactDetails> objResponseWrapper = null;
            List<UserContactDetails> lstUpdateUser = new List<UserContactDetails>();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                UserContactDetails result = ObjCustomer.UpdateUserDetails(user);
                if (result != null)
                {
                    //User updated successfully.
                    lstUpdateUser.Clear();
                    lstUpdateUser.Add(new UserContactDetails { UserID = result.UserID, ProfileImageUrl = result.ProfileImageUrl, UserName = result.UserName });
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(lstUpdateUser, HttpStatusCode.OK, true, "Success", "User Updated Successfully");
                }
                else
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(null, HttpStatusCode.NotFound, false, "Fail", "User Updation Failed");


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(null, HttpStatusCode.InternalServerError, false, "Error while User Updating", ex.Message);
            }
        }

        /// <summary>
        /// Update User Profile
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateUserPasswordDetails")]
        public ResponseWrapperForAddUpdate<UserContactDetails> UpdateUserPasswordDetails([FromBody] UpdateUserPasswordModel user)
        {
            // Return Object
            ResponseWrapperForAddUpdate<UserContactDetails> objResponseWrapper = null;
            List<UserContactDetails> lstUpdateUser = new List<UserContactDetails>();
            try
            {
                UserManagementProvider ObjCustomer = new UserManagementProvider();
                // Decrypt password and updated in db 
                user.Password = EncryptDecrypt.Decrypt(user.Password);
                UserContactDetails result = ObjCustomer.UpdateUserPasswordDetails(user);
                if (result != null)
                {
                    //User updated successfully.
                    lstUpdateUser.Clear();
                    lstUpdateUser.Add(new UserContactDetails { UserID = result.UserID, ProfileImageUrl = result.ProfileImageUrl, UserName = result.UserName, Message = result.Message });

                    // Email sending for register user 
                    mstUser objUser = new mstUser();
                    using (var context = new WYRREntities())
                    {
                        objUser = context.mstUsers.Where(x => x.PhoneNumber.ToLower().Trim() == user.MobileNumber.ToLower().Trim()).FirstOrDefault();
                    }

                    ResultModel resultUser = ObjCustomer.SendEmail(objUser, Common.EmailType.ChangePassword, string.Empty);
                    if (resultUser.IsSuccess)
                        return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(lstUpdateUser, HttpStatusCode.OK, true, "Success", result.Message);
                    else
                    {
                        return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(null, HttpStatusCode.NotFound, false, "Error occured while sending mail", "User updation failed");
                    }
                   
                }
                else
                    return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(null, HttpStatusCode.NotFound, false, "Fail", "User updation failed");


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapperForAddUpdate<UserContactDetails>(null, HttpStatusCode.InternalServerError, false, "Error while User Updating", ex.Message);
            }
        }

        /// <summary>
        /// Verifry User Account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifyUserAccount")]
        public ResponseWrapper<ResponseModel> VerifyUserAccount(UserAccountVerficationModel user)
        {
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                mstUser objUser = new mstUser();
                using (var context = new WYRREntities())
                {
                    objUser = context.mstUsers.Where(x => x.UserId == user.UserId).Where(x => x.IsDeleted == false).FirstOrDefault();
                }

                if (objUser != null)
                {
                    #region ValidUser

                    ResultModel result = ObjUser.VerfiyRegistredAccount(objUser, user);

                    if (result.IsSuccess)
                    {
                        List<ResponseModel> lstResponseModel = new List<ResponseModel>();
                        lstResponseModel.Add(new ResponseModel { IsSuccess = result.IsSuccess, Message = result.Message });

                        return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, result.Message, null);
                    }
                    else
                        return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.NotFound, "Fail", false, result.Message, null);

                    #endregion

                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.NotFound, "Fail", false, "Please enter valid User ID", null);
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Error occured while sending mail", false, ex.Message, null);
            }




        }


        /// <summary>
        /// Get OTP
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetOTP")]
        public ResponseWrapper<ResponseOTPModel> GetOTP(UserOTPModel user)
        {
            ResponseWrapper<ResponseOTPModel> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];

                mstUser objUser = new mstUser();
                using (var context = new WYRREntities())
                {
                    if (user.PhoneNumber == null || user.PhoneNumber == string.Empty || user.PhoneNumber == "null")
                    {
                        objUser = context.mstUsers.Where(x => x.UserId == user.UserId).Where(x => x.IsDeleted == false).FirstOrDefault();
                    }
                    else
                    {
                        //Forgot Password case
                        objUser = context.mstUsers.Where(x => x.PhoneNumber == user.PhoneNumber).Where(x => x.IsDeleted == false).FirstOrDefault();

                    }
                }

                if (objUser != null)
                {
                    #region ValidUser

                    ResultOTPModel result = ObjUser.GetOTP(objUser);

                    if (result.IsSuccess)
                    {
                        List<ResponseOTPModel> lstResponseModel = new List<ResponseOTPModel>();
                        lstResponseModel.Add(new ResponseOTPModel { IsSuccess = result.IsSuccess, Message = result.Message, OTP = result.OTP });

                        #region SMSGateway

                        AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                        PublishRequest publishRequest = new PublishRequest();
                        publishRequest.Message = String.Format(SMSTemplates.otpSMS, result.OTP);
                         publishRequest.PhoneNumber = "+" + objUser.CountryCode + objUser.PhoneNumber;
                        // publishRequest.PhoneNumber = "+001" + "3059277315";
                        PublishResponse results = smsClient.Publish(publishRequest);

                        #endregion


                        return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, result.Message, null);
                    }
                    else
                        return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(null, false, HttpStatusCode.NotFound, "Fail", false, result.Message, null);

                    #endregion


                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(null, false, HttpStatusCode.NotFound, "Fail", false, "Please enter valid phone number", null);
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(null, false, HttpStatusCode.InternalServerError, "Error occured while genrating OTP", false, ex.Message, null);
            }




        }


        /// <summary>
        /// Verify OTP
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifyOTP")]
        public ResponseWrapper<ResponseOTPModel> VerifyOTP(UserVerifyOTPModel user)
        {
            ResponseWrapper<ResponseOTPModel> objResponseWrapper = null;
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();


                #region ValidUser

                ResultOTPModel result = ObjUser.VerifyOTP(user);

                if (result.IsSuccess)
                {
                    List<ResponseOTPModel> lstResponseModel = new List<ResponseOTPModel>();
                    lstResponseModel.Add(new ResponseOTPModel { IsSuccess = result.IsSuccess, Message = result.Message, OTP = result.OTP });

                    return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, result.Message, null);
                }
                else
                    return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(null, false, HttpStatusCode.NotFound, "Fail", false, result.Message, null);

                #endregion


            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<ResponseOTPModel>(null, false, HttpStatusCode.InternalServerError, "Error occured while verifying OTP", false, ex.Message, null);
            }




        }

        /// <summary>
        /// Get All Country Code
        /// </summary>
        /// <param name="searchuser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetAllCountryCode")]
        public ResponseWrapper<CountryDetails> GetAllCountryCode()
        {
            ResponseWrapper<CountryDetails> objResponseWrapper = null;
            List<CountryDetails> lstCountryDetails = new List<CountryDetails>();
            try
            {
                UserManagementProvider ObjUser = new UserManagementProvider();

                lstCountryDetails = ObjUser.GetAllCountry();

                if (lstCountryDetails.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<CountryDetails>(lstCountryDetails, false, HttpStatusCode.OK, "Success", true, "Country Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<CountryDetails>(null, false, HttpStatusCode.OK, "Fail", true, "Country Details", null);
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<CountryDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstCountryDetails = null;
            }


        }

        /// <summary>
        /// Get All Transaction Settings
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetAllTransactionSettings")]
        public ResponseWrapperObject<TransactionSettings> GetAllTransactionSettings()
        {
            ResponseWrapperObject<TransactionSettings> objResponseWrapper = null;
            //List<CountryDetails> lstCountryDetails = new List<CountryDetails>();
            TransactionSettings txnSettings = new TransactionSettings();
            try
            {
                UserTransactionManagementProvider ObjTxnSetting = new UserTransactionManagementProvider();

                txnSettings = ObjTxnSetting.GetTransactionSettingDetails().Select(r=> new TransactionSettings { EnableAddPaypal=Convert.ToInt32(r.EnableAddPayPal),EnableWithdrawPaypal= Convert.ToInt32(r.EnableWithdrawPayPal), EnableAddCoinbase = Convert.ToInt32(r.EnableAddCoinbase),CoinbaseSendLimit=Convert.ToString(r.CoinbaseSendLimit) }).FirstOrDefault();
               return objResponseWrapper = new ResponseWrapperObject<TransactionSettings>(txnSettings, false, HttpStatusCode.OK, "Success", true, "Transaction Settings", null);
                
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapperObject<TransactionSettings>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                txnSettings = null;
            }


        }

        /// <summary>
        /// Get All Transaction Settings
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetS3BucketDetails")]
        public ResponseWrapper<AWSCredentials> GetS3BucketDetails()
        {
            ResponseWrapper<AWSCredentials> objResponseWrapper = null;
            List<AWSCredentials> lstawsCreds = new List<AWSCredentials>();
            AWSCredentials awsCreds = new AWSCredentials();
            try
            {
                awsCreds.ClientID = EncryptDecrypt.Encrypt("AKIA44DR6Q4POB53XHXP"); ;
                awsCreds.SecretKey = EncryptDecrypt.Encrypt("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");
                lstawsCreds.Add(awsCreds);
                return objResponseWrapper = new ResponseWrapper<AWSCredentials>(lstawsCreds, false, HttpStatusCode.OK, "Success", true, "Transaction Settings", null);

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<AWSCredentials>(lstawsCreds, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                awsCreds = null;
            }


        }

    }
}