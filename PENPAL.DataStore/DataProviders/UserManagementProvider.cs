using PENPAL.DataStore.SMSHelper;
using PENPAL.DataStore.Templates;
using PENPAL.VM.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using PENPAL.DataStore.Utility;
using static PENPAL.DataStore.Utility.Common;
using PENPAL.DataStore.APIModel;
using System.IO;
using System.Web.Hosting;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json;
using static PENPAL.DataStore.StellarModel.AccountModel;
using static PENPAL.DataStore.StellarModel.StellarAPIErrorModel;
using PENPAL.DataStore.EmailHelper;

namespace PENPAL.DataStore.DataProviders
{
    public class UserManagementProvider
    {
        public static WYRREntities db = new WYRREntities();
        public UserRegistrationAPIModel RegisterUser(UserModel user)
        {
            UserRegistrationAPIModel result = new UserRegistrationAPIModel();

            try
            {
                mstUser objUser = new mstUser();

                using (var context = new WYRREntities())
                {

                    string DecryptedPassword = user.Password;//EncryptDecrypt.Decrypt(user.Password);
                    var checkUser = new mstUser();
                     //added email to be checked
                     if(user.EmailAddress!=string.Empty)
                    {
                        checkUser = context.mstUsers.Where(x => x.UserEmail == user.EmailAddress && x.IsVerified == true).FirstOrDefault();
                    }
                    else
                    {
                        checkUser = context.mstUsers.Where(x => x.PhoneNumber == user.PhoneNumber && x.IsVerified == true).FirstOrDefault();
                    }
                      
                    


                    if (checkUser == null)
                    {
                        #region NewUser


                        #region StellarAccountGenration

                        var t = GetAccountDetails();
                        var task = t.Result;

                        #endregion


                        if (task.Publickey != string.Empty && task.SecretKey != string.Empty && task.Publickey != "" && task.SecretKey != "")
                        {
                            #region NewUser

                            #region AccountNumberEncryption

                            string Publickey = EncryptDecrypt.Encrypt(task.Publickey);
                            string SecretKey = EncryptDecrypt.Encrypt(task.SecretKey);

                            #endregion

                            objUser.CountryCode = user.CountryCode;
                            objUser.PhoneNumber = user.PhoneNumber;
                            objUser.Name = user.UserName;
                            objUser.Password = DecryptedPassword;//user.Password;
                            objUser.ConfirmPassword = DecryptedPassword;//user.Password;
                            objUser.IsSocialLogin = user.IsSocialLogin;
                            objUser.CreatedDate = DateTime.UtcNow;
                            objUser.IsDeleted = false;
                            objUser.UserEmail = user.EmailAddress;
                            objUser.UpdatedDate = DateTime.UtcNow;
                            objUser.UpdatedBy = 1;
                            //objUser.PublicKey = task.Publickey;
                            //objUser.SecretKey = task.SecretKey;
                            objUser.PublicKey = Publickey;
                            objUser.SecretKey = SecretKey;
                            objUser.DefaultCurrency = user.DefaultCurrency;
                            objUser.IsActive = true;
                            objUser.UserUniqueId = user.UniqueUserId;
                            objUser.IsVerified = false;
                            objUser.IsAdmin = false;
                            context.mstUsers.Add(objUser);
                            context.SaveChanges();
                            user.UserID = objUser.UserId;


                            result.IsSuccess = true;
                            result.RecordID = Convert.ToInt32(user.UserID);
                            //result.PublicKey = task.Publickey;
                            result.PublicKey = Publickey;
                            #endregion

                            #region ForProfileImage

                            if (user.ProfileImage != null)
                            {
                                #region ByteArrayToImage

                                var updateUser = context.mstUsers.Where(x => x.UserId == result.RecordID).FirstOrDefault();

                                if (updateUser != null)
                                {
                                    string image = updateUser.UserId + ".jpeg";

                                    string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];

                                    //get hosting environment path
                                    var uploads = HostingEnvironment.MapPath("~/profileimages/");

                                    //check if director exists or not
                                    if (!Directory.Exists(uploads))
                                    {
                                        Directory.CreateDirectory(uploads);
                                    }

                                    byte[] data = Convert.FromBase64String(user.ProfileImage);
                                    BinaryWriter writer = new BinaryWriter(File.OpenWrite(uploads + image));
                                    writer.Write(data);
                                    writer.Flush();
                                    writer.Close();

                                    string imageurl = ProfileImageURL + image;

                                    updateUser.ImageUrl = "profileimages/" + image;
                                    context.SaveChanges();
                                    result.ProfileImageUrl = ProfileImageURL + updateUser.ImageUrl;

                                }



                                #endregion
                            }
                            else
                            {
                                var updateUser = context.mstUsers.Where(x => x.UserId == result.RecordID).FirstOrDefault();

                                if (updateUser != null)
                                {
                                    string image = "default" + ".png";
                                    string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];

                                    updateUser.ImageUrl = "profileimages/" + image;
                                    context.SaveChanges();
                                    result.ProfileImageUrl = ProfileImageURL + updateUser.ImageUrl;

                                }
                            }

                            #endregion

                            #region OTPGenration

                            string OTP = GenerateOTP(user.UserID);
                            if (OTP != string.Empty)
                            {
                                //if (Convert.ToBoolean(WebConfigurationManager.AppSettings["SMSEnabled"]))
                                //    SMSManagement.SendSMS(WebConfigurationManager.AppSettings["SMSUser"], WebConfigurationManager.AppSettings["SMSPassword"], WebConfigurationManager.AppSettings["SMSSenderID"], user.PhoneNumber, String.Format(SMSTemplates.otpSMS, OTP));
                                result.OTP = OTP;

                            }

                            #endregion


                        }
                        else
                        {
                            result.Message = "Error occured while creating account, Please try again later.";
                        }

                        #endregion

                    }
                    else
                    {
                        #region ExistingUser

                        result.Message = "User already exists";

                        #endregion

                    }


                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }

        public async Task<UserStellarAccountModel> GetAccountDetails()
        {
            UserStellarAccountModel user = new UserStellarAccountModel();
            try
            {
                #region StellarAPI
                //get object for token
                RootObject accountDetails = new RootObject();

                string apiBaseUri = ConfigurationManager.AppSettings["StellarAPIBaseUrl"].ToString();
                var PostClient = new HttpClient();
                PostClient.BaseAddress = new Uri(apiBaseUri);
                var response = PostClient.GetAsync(apiBaseUri + "/CreateNewAccount").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    List<Result> result = JsonConvert.DeserializeObject<RootObject>(responseJson).Result;

                    user.Publickey = result[0].publicKey;
                    user.SecretKey = result[0].secretKey;

                }




                #endregion
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return user;

        }

        public UserValidationAPIModel Login(string PhoneNumber, string Password, bool isEmail)
        {
            UserValidationAPIModel result = new UserValidationAPIModel();
            string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
            try
            {
                using (var context = new WYRREntities())
                {

                    //for testing purpose

                    string DecryptedPassword = EncryptDecrypt.Decrypt(Password);
                    var checkUser= new mstUser();
                    // var checkUser = context.mstUsers.Where(x => x.PhoneNumber == PhoneNumber && x.Password == Password && x.IsDeleted == false && x.IsVerified == true).FirstOrDefault();
                    //Check user based on email or phone number 18-06-2019
                    if (isEmail)
                    {
                        checkUser = context.mstUsers.AsEnumerable().Where(x => x.UserEmail == PhoneNumber && x.Password == DecryptedPassword && x.IsDeleted == false && x.IsVerified == true).FirstOrDefault();
                    }
                    else { 
                      checkUser = context.mstUsers.AsEnumerable().Where(x => x.PhoneNumber == PhoneNumber && x.Password == DecryptedPassword && x.IsDeleted == false && x.IsVerified == true).FirstOrDefault();
                    }
                    if (checkUser != null)
                    {
                        checkUser.UpdatedDate = DateTime.UtcNow;
                        context.SaveChanges();

                        result.IsSuccess = true;
                        result.UserID = Convert.ToInt32(checkUser.UserId);
                        result.UserName = checkUser.Name;
                        result.PublicKey = checkUser.PublicKey;
                        result.SecretKey = checkUser.SecretKey;
                        result.PhoneNumber = checkUser.PhoneNumber;
                        result.EmailID = checkUser.UserEmail;
                        if (checkUser.ImageUrl != string.Empty)
                        {
                            result.ProfileImageUrl = ProfileImageURL + checkUser.ImageUrl;
                        }
                        result.Currency = checkUser.DefaultCurrency;

                    }

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }

        public ResultModel SendEmail(mstUser objUser, EmailType emailType, string TrnAmount)
        {
            ResultModel result = new ResultModel();
            bool IsEmailSend = false;
            try
            {
                switch (emailType)
                {
                    case EmailType.ForgotPassword:
                        IsEmailSend = SendUserEmail(EmailType.ForgotPassword, objUser, string.Empty);
                         break;

                    case EmailType.ChangePassword:
                        IsEmailSend = SendUserEmail(EmailType.ChangePassword, objUser, string.Empty);
                        break;
                    case EmailType.RegisterUser:
                        IsEmailSend = SendUserEmail(EmailType.RegisterUser, objUser, TrnAmount);
                        break;

                    case EmailType.PaypalPaymentSuccess:
                        IsEmailSend = SendUserEmail(EmailType.PaypalPaymentSuccess, objUser, TrnAmount);
                        break;

                    case EmailType.ChangellyTickets:
                        IsEmailSend = SendUserEmail(EmailType.ChangellyTickets, objUser, TrnAmount);
                        break;

                    case EmailType.Exception:
                        break;

                    default:
                        break;
                }

                if (IsEmailSend)
                {
                    result.IsSuccess = true;
                    result.Message = string.Format(UserMessages.MailSentMessage, "");
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = string.Format(UserMessages.MailSentFailedMessage, "");
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }

        //public ResultModel SendRegisterUserEmail(mstUser objUser)
        //{
        //    ResultModel result = new ResultModel();
        //    try
        //    {
        //        if (SendUserEmail(EmailType.RegisterUser, objUser))
        //        {
        //            result.IsSuccess = true;
        //            result.Message = string.Format(UserMessages.MailSentMessage, "");
        //        }
        //        else
        //        {
        //            result.IsSuccess = false;
        //            result.Message = string.Format(UserMessages.MailSentFailedMessage, "");
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        Logger.Log(ex, Logger.LogType.Error);
        //    }
        //    return result;
        //}

        public List<UserContactDetails> GetAllContactDetails(UserContactModel contact)
        {
            List<UserContactDetails> lstUserContact = new List<UserContactDetails>();
            // bool isNextPagePresent = false;
            try
            {
                List<mstUser> lstUser = new List<mstUser>();

                using (var context = new WYRREntities())
                {
                    lstUser = context.mstUsers.Where(x => x.UserId != contact.UserID && x.IsDeleted == false && x.IsAdmin == false && x.IsVerified == true).OrderBy(x => x.Name).ToList();

                    //lstUser = Pagination<mstUser>.GetFilterResult(lstUser);
                    // isNextPage = isNextPagePresent;


                    #region GetMatchedContactList

                    string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
                    foreach (var user in lstUser)
                    {

                        UserContactDetails objUserContact = new UserContactDetails();
                        //add such contact in one final list
                        objUserContact.UserID = Convert.ToInt32(user.UserId);
                        objUserContact.UserName = user.Name;
                        objUserContact.ProfileImageUrl = ProfileImageURL + user.ImageUrl;
                        objUserContact.PublicKey = user.PublicKey;
                        objUserContact.SecretKey = user.SecretKey;
                        objUserContact.PhoneNumber = user.PhoneNumber;
                        objUserContact.EmailId = user.UserEmail;
                        objUserContact.UserUniqueId = user.UserUniqueId;
                        lstUserContact.Add(objUserContact);


                    }




                    #endregion

                    #region BytearraytoImage

                    //foreach (var contact in lstUserContact)
                    //{
                    //    if (contact.ProfileImageUrl != null)
                    //    {
                    //        string image = contact.UserID + ".jpeg";

                    //        string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];

                    //        //Get hosting environment path
                    //        var uploads = HostingEnvironment.MapPath("~/ProfileImages/");

                    //        //Check if director exists or not
                    //        if (!Directory.Exists(uploads))
                    //        {
                    //            Directory.CreateDirectory(uploads);
                    //        }

                    //        byte[] data = Convert.FromBase64String(contact.ProfileImageUrl);
                    //        BinaryWriter writer = new BinaryWriter(File.OpenWrite(uploads + image));
                    //        writer.Write(data);
                    //        writer.Flush();
                    //        writer.Close();

                    //        string ImageUrl = ProfileImageURL + image;

                    //        var checkUser = context.mstUsers.Where(x => x.UserId == contact.UserID).FirstOrDefault();

                    //        if (checkUser != null)
                    //        {
                    //            checkUser.ImageUrl = "ProfileImages/" + image;
                    //            context.SaveChanges();
                    //            contact.ProfileImageUrl = ProfileImageURL + checkUser.ImageUrl;
                    //        }

                    //    }


                    //}


                    #endregion


                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            // isNextPage = isNextPagePresent;
            return lstUserContact;

        }

        public List<UserContactDetails> GetAllContactDetails_V2(UserContactModel_V2 contact)
        {
            List<UserContactDetails> lstUserContact = new List<UserContactDetails>();
            // bool isNextPagePresent = false;
            try
            {
                List<mstUser> lstUser = new List<mstUser>();

                string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];

                using (var context = new WYRREntities())
                {
                    if (contact.IsFrequentChat)
                    {

                        //var lstRecentUserContact = context.GetRecentChatUser(contact.UserID).Where(x => x.UserId != contact.UserID).ToList();
                        var lstRecentUserContact = context.GetRecentChatUser(contact.UserID).ToList();

                        foreach (var user in lstRecentUserContact)
                        {

                            UserContactDetails objUserContact = new UserContactDetails();
                            //add such contact in one final list
                            objUserContact.UserID = Convert.ToInt32(user.UserId);
                            objUserContact.UserName = user.Name;
                            objUserContact.ProfileImageUrl = ProfileImageURL + user.ImageUrl;
                            objUserContact.PublicKey = user.PublicKey;
                            objUserContact.SecretKey = user.SecretKey;
                            objUserContact.PhoneNumber = user.PhoneNumber;
                            objUserContact.EmailId = user.UserEmail;
                            objUserContact.UserUniqueId = user.UserUniqueId;
                            lstUserContact.Add(objUserContact);

                        }


                    }
                    else
                    {
                        lstUser = context.mstUsers.Where(x => x.UserId != contact.UserID && x.IsDeleted == false && x.IsAdmin == false && x.IsVerified == true).OrderBy(x => x.Name).ToList();

                        #region GetMatchedContactList

                        // string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
                        foreach (var user in lstUser)
                        {

                            UserContactDetails objUserContact = new UserContactDetails();
                            //add such contact in one final list
                            objUserContact.UserID = Convert.ToInt32(user.UserId);
                            objUserContact.UserName = user.Name;
                            objUserContact.ProfileImageUrl = ProfileImageURL + user.ImageUrl;
                            objUserContact.PublicKey = user.PublicKey;
                            objUserContact.SecretKey = user.SecretKey;
                            objUserContact.PhoneNumber = user.PhoneNumber;
                            objUserContact.EmailId = user.UserEmail;
                            objUserContact.UserUniqueId = user.UserUniqueId;
                            lstUserContact.Add(objUserContact);


                        }




                        #endregion
                    }





                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            // isNextPage = isNextPagePresent;
            return lstUserContact;

        }

        public List<UserContactDetails> GetSearchedContatcDetails(SearchedUserContactModel searchuser)
        {
            List<UserContactDetails> lstUserContact = new List<UserContactDetails>();
            try
            {
                List<mstUser> lstUser = new List<mstUser>();
                string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
                using (var context = new WYRREntities())
                {
                    lstUser = context.mstUsers.Where(x => x.Name.Contains(searchuser.SearchedText) || x.PhoneNumber.Contains(searchuser.SearchedText) || x.UserUniqueId.Contains(searchuser.SearchedText) && x.UserId != searchuser.UserID).ToList();

                    foreach (var item in lstUser)
                    {
                        UserContactDetails objUser = new UserContactDetails();

                        objUser.UserID = Convert.ToInt32(item.UserId);
                        objUser.UserName = item.Name;
                        objUser.ProfileImageUrl = ProfileImageURL + item.ImageUrl;
                        objUser.PublicKey = item.PublicKey;
                        objUser.SecretKey = item.SecretKey;
                        objUser.PhoneNumber = item.PhoneNumber;
                        objUser.UserUniqueId = item.UserUniqueId;
                        lstUserContact.Add(objUser);
                    }

                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lstUserContact;

        }

        public UserProfileDetails GetUserProfile(int UserID)
        {
            UserProfileDetails result = new UserProfileDetails();
            string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
            try
            {
                using (var context = new WYRREntities())
                {
                    var checkUser = context.mstUsers.Where(x => x.UserId == UserID).FirstOrDefault();

                    if (checkUser != null)
                    {
                        result.UserID = Convert.ToInt32(checkUser.UserId);
                        result.UserName = checkUser.Name;
                        result.ProfileImageUrl = ProfileImageURL + checkUser.ImageUrl;
                        result.PhoneNumber = checkUser.PhoneNumber;
                        result.DefaultCurrency = checkUser.DefaultCurrency;
                        result.EmailAddress = checkUser.UserEmail;
                        result.Publickey = checkUser.PublicKey;
                        result.Secretkey = checkUser.SecretKey;
                    }

                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;

        }

        public ResultModel VerfiyRegistredAccount(mstUser user, UserAccountVerficationModel verfiyuser)
        {
            ResultModel result = new ResultModel();
            try
            {
                using (var context = new WYRREntities())
                {
                    var checkOTP = context.OTPTransactions.Where(x => x.UserId == user.UserId && x.OTP == verfiyuser.OTP).FirstOrDefault();
                    if (checkOTP != null)
                    {
                        if (verfiyuser.IsLinkStellarAccount)
                        {
                            #region LinkStellarAccountDetails

                            string Publickey = EncryptDecrypt.Decrypt(verfiyuser.Publickey);

                            var t = GetUserWalletDetails(Publickey);
                            var task = t.Result;

                            if (task.IsSuccess)
                            {
                                #region UpdateUserEntry

                                var updateuser = context.mstUsers.Where(x => x.UserId == user.UserId && x.IsDeleted == false).FirstOrDefault();
                                updateuser.PublicKey = verfiyuser.Publickey;
                                updateuser.SecretKey = verfiyuser.Secretkey;
                                updateuser.IsVerified = true;
                                context.SaveChanges();

                                #endregion


                                #region SendEmailFunctionality

                                //for email functionality
                                EmailManagement.SendEmail_Background(user.UserEmail, string.Empty, string.Empty, "WYRR Credentials", String.Format(EmailTemplates.UserRegistrationEmail(), user.PhoneNumber, user.Password));


                                #endregion



                                result.IsSuccess = true;
                                result.Message = "Valid Stellar Account Details";
                            }
                            else
                            {
                                result.Message = task.Message;
                            }


                            #endregion
                        }
                        else
                        {
                            #region VerfiryUserAccount

                            var updateuser = context.mstUsers.Where(x => x.UserId == user.UserId && x.IsDeleted == false).FirstOrDefault();

                            updateuser.IsVerified = true;

                            //user.IsVerified = true;
                            context.SaveChanges();
                            result.IsSuccess = true;
                            result.Message = "User verified successfully";


                            #endregion

                        }
                        #region InsertEntryInUserTransactionSetting

                        trnUserTransactionSetting setting = new trnUserTransactionSetting();

                        setting.UserId = verfiyuser.UserId;
                        setting.TransactionAmountPerDay = 100;
                        setting.TransactionPerDay = 100;
                        context.trnUserTransactionSettings.Add(setting);
                        context.SaveChanges();

                        #endregion

                    }
                    else
                    {
                        result.Message = "Invalid OTP";
                    }


                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;


        }

        public ResultOTPModel GetOTP(mstUser user)
        {
            ResultOTPModel result = new ResultOTPModel();
            try
            {
                // Random rnd = new Random();
                // int OTPValue = rnd.Next(1000, 9999);
                //string OTP = Convert.ToString(OTPValue);
                string OTP = GenerateOTP(user.UserId);
                if (OTP != string.Empty)
                {
                    //if (Convert.ToBoolean(WebConfigurationManager.AppSettings["SMSEnabled"]))
                    //    SMSManagement.SendSMS(WebConfigurationManager.AppSettings["SMSUser"], WebConfigurationManager.AppSettings["SMSPassword"], WebConfigurationManager.AppSettings["SMSSenderID"], user.PhoneNumber, String.Format(SMSTemplates.otpSMS, OTP));
                    result.OTP = OTP;
                    result.IsSuccess = true;
                    result.Message = "OTP generated successfully";
                }
                else
                {

                    result.Message = "Error occured while generating OTP ";
                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }

        public ResultOTPModel VerifyOTP(UserVerifyOTPModel user)
        {
            ResultOTPModel result = new ResultOTPModel();
            try
            {
                OTPTransaction verifyuser = new OTPTransaction();
                using (var context = new WYRREntities())
                {
                    if (user.UserId != 0)
                    {
                        verifyuser = context.OTPTransactions.Where(x => x.UserId == user.UserId && x.OTP == user.OTP).FirstOrDefault();
                    }
                    else
                    {
                        //ForgotPAssword case
                        verifyuser = context.OTPTransactions.Where(x => x.OTP == user.OTP).FirstOrDefault();
                    }


                    if (verifyuser != null)
                    {
                        verifyuser.isVerified = true;
                        context.SaveChanges();
                        result.IsSuccess = true;
                        result.Message = "OTP verified successfully";
                        result.OTP = verifyuser.OTP;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Invalid OTP";
                        result.OTP = null;
                    }

                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;

        }

        public List<CountryDetails> GetAllCountry()
        {
            List<CountryDetails> lstUserCountry = new List<CountryDetails>();
            string IconImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];
            try
            {
                List<mstCountry> lstCountry = new List<mstCountry>();

                using (var context = new WYRREntities())
                {
                    lstCountry = context.mstCountries.Where(x => x.IsDeleted == false).ToList();

                    foreach (var item in lstCountry)
                    {
                        CountryDetails objCountry = new CountryDetails();
                        objCountry.Id = item.Id;
                        objCountry.CountryName = item.CountryName;
                        objCountry.CountryCode = item.CountryCode;
                        objCountry.IconImage = IconImageURL + item.IconImage;
                        lstUserCountry.Add(objCountry);

                    }

                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lstUserCountry;

        }

        public List<ResultModel> AddEditNewFeed(newsFeedModel feed)
        {
            List<ResultModel> lstresult = new List<ResultModel>();
            ResultModel result = new ResultModel();
            try
            {
                using (var context = new WYRREntities())
                {
                    if(feed.User_feedID != 0)
                    {
                        newsFeed updateFeed =  context.newsFeeds.Where(s=>s.feedID==feed.User_feedID).FirstOrDefault();
                        updateFeed.title = feed.Title;
                        updateFeed.description = feed.Description;
                        updateFeed.modified_at = DateTime.UtcNow;
                        updateFeed.modified_by = feed.UserID;
                        updateFeed.isApproved = false;
                        //updateFeed.userID = feed.UserID;
                    }
                    else
                    {
                        newsFeed newFeed = new newsFeed();
                        newFeed.title = feed.Title;
                        newFeed.description = feed.Description;
                        newFeed.created_at = DateTime.UtcNow;
                        newFeed.created_by = feed.UserID;
                        newFeed.modified_at = DateTime.UtcNow;
                        newFeed.modified_by = feed.UserID;
                        newFeed.userID = feed.UserID;
                        newFeed.isApproved = false;
                        context.newsFeeds.Add(newFeed);
                    }
                    context.SaveChanges();
                    result.IsSuccess = true;
                    result.Message = "Post is sent for approval. Once approved it will display in the post section";
                    lstresult.Add(result);
                    return lstresult;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                lstresult.Add(result);
                Logger.Log(ex, Logger.LogType.Error);
                return lstresult;
            }
        }

        public List<GetNewsFeeds_Result> GetNewsFeedforPost(long UserID)
        {
            List<GetNewsFeeds_Result> lstNewsFeed = new List<GetNewsFeeds_Result>();
            try
            {

                using (var context = new WYRREntities())
                {
                    //get users from SP GetUsersByName
                    lstNewsFeed = context.GetNewsFeeds(UserID).ToList();

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lstNewsFeed;
        }

        public List<ResultModel> RemoveNewsFeed(long feedID)
        {
            List<ResultModel> lstresult = new List<ResultModel>();
            ResultModel result = new ResultModel();
            try
            {
                using (var context = new WYRREntities())
                { 
                    newsFeed removeFeed = context.newsFeeds.Where(s => s.feedID == feedID).FirstOrDefault();
                    context.newsFeeds.Remove(removeFeed);
                    context.SaveChanges();
                    result.IsSuccess = true;
                    result.Message = "This post has been deleted";
                    lstresult.Add(result);
                    return lstresult;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                Logger.Log(ex, Logger.LogType.Error);
                lstresult.Add(result);
                return lstresult;
            }
        }

        #region Pubnub
        //Get Chat users by Name
        public List<UserSearchDetails> GetUsersByName(SearchedUserModel searchUser)
            {
                List<UserSearchDetails> lstUserSearchDetails = new List<UserSearchDetails>();
                try
                {

                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        var lstUserSearchDetail = context.GetUsersByName(searchUser.UserID, searchUser.SearchedText).ToList();


                        #region AddUserstoList

                        foreach (var item in lstUserSearchDetail)
                        {
                            UserSearchDetails objUserDetail = new UserSearchDetails();
                            objUserDetail.UserID = item.UserId;
                            objUserDetail.UserName = item.Name;
                            objUserDetail.ConnectionStatus = item.connection_status;
                            objUserDetail.DefaultCurrency = item.DefaultCurrency;
                            objUserDetail.EmailAddress = item.UserEmail;
                            objUserDetail.PhoneNumber = item.PhoneNumber;
                            objUserDetail.ProfileImageUrl = item.ImageUrl;
                            objUserDetail.Publickey = item.PublicKey;
                            objUserDetail.ConnectionID = item.connection_id;
                            lstUserSearchDetails.Add(objUserDetail );
                        }

                        #endregion

                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return lstUserSearchDetails;
            }

            //Send invite to chat 
            public List<SendChatInviteToUser_Result> sendInviteToUser(InviteUserModel inviteUser)
            {
            List<SendChatInviteToUser_Result> inviteresponse = new List<SendChatInviteToUser_Result>();
                try
                {
                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        inviteresponse = context.SendChatInviteToUser(inviteUser.SenderID, inviteUser.ReceiverID).ToList();

                    

                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return inviteresponse;
            }

            //Get Chat Invites By User
            public List<UserSearchDetails> GetChatInvitesByUser(ChatUserModel chatUser)
            {
                List<UserSearchDetails> lstUserSearchDetails = new List<UserSearchDetails>();
                try
                {

                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        var lstUserSearchDetail = context.GetChatInvitesForUser(chatUser.UserID).ToList();


                        #region AddUserstoList

                        foreach (var item in lstUserSearchDetail)
                        {
                            UserSearchDetails objUserDetail = new UserSearchDetails();
                            objUserDetail.UserID = item.UserId;
                            objUserDetail.UserName = item.Name;
                            objUserDetail.ConnectionStatus = item.connection_status;
                            objUserDetail.DefaultCurrency = item.DefaultCurrency;
                            objUserDetail.EmailAddress = item.UserEmail;
                            objUserDetail.PhoneNumber = item.PhoneNumber;
                            objUserDetail.ProfileImageUrl = item.ImageUrl;
                            objUserDetail.Publickey = item.PublicKey;
                            objUserDetail.ConnectionID = item.connection_id;
                            lstUserSearchDetails.Add(objUserDetail);
                        }

                        #endregion

                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return lstUserSearchDetails;
            }


            //Get Chat Invites By User
            public List<UserSearchDetails> GetRecentChatsByUser(ChatUserModel chatUser)
            {
                List<UserSearchDetails> lstUserSearchDetails = new List<UserSearchDetails>();
                try
                {

                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        var lstUserSearchDetail = context.GetRecentChats(chatUser.UserID).ToList();


                        #region AddUserstoList

                        foreach (var item in lstUserSearchDetail)
                        {
                            UserSearchDetails objUserDetail = new UserSearchDetails();
                            objUserDetail.UserID = item.UserId;
                            objUserDetail.UserName = item.Name;
                            objUserDetail.DefaultCurrency = item.DefaultCurrency;
                            objUserDetail.EmailAddress = item.UserEmail;
                            objUserDetail.PhoneNumber = item.PhoneNumber;
                            objUserDetail.ProfileImageUrl = item.ImageUrl;
                            objUserDetail.Publickey = item.PublicKey;
                            objUserDetail.ConnectionID = item.connection_id;
                            lstUserSearchDetails.Add(objUserDetail);
                        }

                        #endregion

                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return lstUserSearchDetails;
            }

            //Accept Reject chat 
            public List<AcceptRejectResendChatInvite_Result> AcceptRejectChatInvite(ChatInviteStatus chatStatus)
            {
                List<AcceptRejectResendChatInvite_Result> changeStatusresponse = new List<AcceptRejectResendChatInvite_Result>();
                try
                {
                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        changeStatusresponse = context.AcceptRejectResendChatInvite(chatStatus.ConnectionID, chatStatus.ConnectStatus).ToList();

                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return changeStatusresponse;
            }

            //Update recent chat date 
            public List<UpdateRecentChat_Result> UpdateRecentChat(ChatInviteStatus chatStatus)
            {
                List<UpdateRecentChat_Result> chatUpdateresponse = new List<UpdateRecentChat_Result>();
                try
                {
                    using (var context = new WYRREntities())
                    {
                        //get users from SP GetUsersByName
                        chatUpdateresponse = context.UpdateRecentChat(chatStatus.ConnectionID).ToList();



                    }
                }
                catch (Exception ex)
                {

                    Logger.Log(ex, Logger.LogType.Error);
                }
                return chatUpdateresponse;
            }

        #endregion


        #region CMSMethods 

        public UserModel ValidateUserName(string username, string Password)
        {
            //UserModel user = new UserModel();
            try
            {
                using (var context = new WYRREntities())
                {
                    //.AsEnumerable()
                    // where userData.UserEmail.ToLower().Trim() == username && userData.Password.Trim() == Password

                    var user = (from userData in context.mstUsers.Where(r => r.UserEmail.ToLower().Trim() == username && r.Password == Password).AsEnumerable()
                                select new UserModel()
                                {
                                    EmailAddress = userData.UserEmail,
                                    UserName = userData.Name,
                                    UserID = userData.UserId,
                                    DefaultCurrency = userData.DefaultCurrency,
                                    IsSocialLogin = userData.IsSocialLogin,
                                    PhoneNumber = userData.PhoneNumber,
                                    UniqueUserId = userData.UserUniqueId

                                }).FirstOrDefault();
                    return user;
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            // return user;

        }

        public List<MasterUserModel> GetStellarMasterUserDetails()
        {
            using (var context = new WYRREntities())
            {
                var users = (from userData in context.StellarMasterUsers.Where(x => x.IsActive == true).AsEnumerable()
                             select new MasterUserModel()
                             {

                                 ID = userData.Id,
                                 PublicKey = EncryptDecrypt.Decrypt(userData.PublicKey),
                                 SecretKey = EncryptDecrypt.Decrypt(userData.SecretKey)
                             }).ToList();






                return users;
            }

        }

        public MasterUserModel GetStellarMasterUserDetails(long userid)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var user = (from userData in context.StellarMasterUsers.AsEnumerable()
                                where userData.Id == userid
                                select new MasterUserModel()
                                {
                                    PublicKey = EncryptDecrypt.Decrypt(userData.PublicKey),
                                    SecretKey = EncryptDecrypt.Decrypt(userData.SecretKey),
                                    ID = userData.Id
                                }).FirstOrDefault();

                    return user;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public int UpdateMasterUserRecordDetails(MasterUserModel userModel)
        {

            using (var context = new WYRREntities())
            {
                int id;

                var userToEdit = context.StellarMasterUsers.Where(s => s.Id == userModel.ID).SingleOrDefault();

                if (userToEdit.PublicKey != EncryptDecrypt.Encrypt(userModel.PublicKey))
                {
                    if (userToEdit != null)
                    {

                        userToEdit.PublicKey = EncryptDecrypt.Encrypt(userModel.PublicKey);
                        userToEdit.SecretKey = EncryptDecrypt.Encrypt(userModel.SecretKey);

                        var result = context.SaveChanges();

                        if (result > 0)
                        {
                            id = 1;
                            return id;
                        }

                    }
                }


                id = -1;
                return id;

            }


        }

        public List<UserCMSModel> GetUserDetails()
        {
            using (var context = new WYRREntities())
            {
                var users = (from userData in context.mstUsers.Where(x => x.IsAdmin == false && x.IsVerified == true).AsEnumerable()
                             select new UserCMSModel()
                             {
                                 UserID = userData.UserId,
                                 UserName = userData.Name,
                                 EmailAddress = userData.UserEmail,
                                 PhoneNumber = "+" + userData.CountryCode + userData.PhoneNumber,
                                 UniqueUserId = userData.UserUniqueId,
                                 DefaultCurrency = userData.DefaultCurrency,
                                 IsSocialLogin = userData.IsSocialLogin,
                                 IsDeleted = userData.IsDeleted,
                                 Updateddatetime = Convert.ToDateTime(userData.UpdatedDate),
                                 LastLoginDateTime = Convert.ToDateTime(userData.UpdatedDate).ToString("dd/MM/yyyy"),
                                 PublicKey = EncryptDecrypt.Decrypt(userData.PublicKey)
                             }).ToList();

                #region AciveInActiveSection

                foreach (var item in users)
                {
                    DateTime today = DateTime.Today;

                    if (item.Updateddatetime.Value.Month == today.Month && item.Updateddatetime.Value.Year == today.Year)
                    {
                        item.IsActive = true;

                    }
                    else
                    {
                        item.IsActive = false;
                    }

                }

                #endregion




                return users;
            }

        }

        public List<UserCMSModel> GetCMSUserDetails()
        {
            using (var context = new WYRREntities())
            {
                var users = (from userData in context.mstUsers.Where(x => x.IsAdmin == true).AsEnumerable()
                             select new UserCMSModel()
                             {
                                 UserID = userData.UserId,
                                 UserName = userData.Name,
                                 EmailAddress = userData.UserEmail,
                                 PhoneNumber = userData.PhoneNumber,
                                 IsDeleted = userData.IsDeleted


                             }).ToList();
                return users;
            }

        }

        public UserCMSModel GetUserDetails(int userid)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var user = (from userData in context.mstUsers.AsEnumerable()
                                where userData.UserId == userid
                                select new UserCMSModel()
                                {
                                    UserName = userData.Name,
                                    EmailAddress = userData.UserEmail,
                                    PhoneNumber = "+" + userData.CountryCode.Trim() + userData.PhoneNumber.Trim(),
                                    UniqueUserId = userData.UserUniqueId,
                                    UserID = userData.UserId,
                                    IsSocialLogin = userData.IsSocialLogin,
                                    DefaultCurrency = userData.DefaultCurrency != null ? userData.DefaultCurrency.Contains(":") ? userData.DefaultCurrency.Split(':')[1].Trim() : userData.DefaultCurrency.Trim() : null,
                                    IsDeleted = userData.IsDeleted

                                }).FirstOrDefault();
                    return user;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public UserCMSModel GetCMSUserDetails(int? userid)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var user = (from userData in context.mstUsers.AsEnumerable()
                                where userData.UserId == userid
                                select new UserCMSModel()
                                {
                                    UserID=userData.UserId,
                                    UserName = userData.Name,
                                    EmailAddress = userData.UserEmail,
                                    PhoneNumber = userData.PhoneNumber.Trim(),
                                    IsDeleted = userData.IsDeleted,
                                    Password=userData.Password

                                }).FirstOrDefault();
                    return user;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        public List<UserAnalysisModel> GetUserAnalysisDetails(UserAnalysisModel user)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var users = (from userData in context.mstUsers.Where(x => x.IsAdmin == false && x.IsDeleted == false && x.IsVerified == true).AsEnumerable()
                                 where (user.FromDate == null || Convert.ToDateTime(userData.CreatedDate.Value.Date) >= user.FromDate.Value.Date) &&
                                 (user.ToDate == null || Convert.ToDateTime(userData.CreatedDate.Value.Date) <= user.ToDate.Value.Date) &&
                                 (string.IsNullOrWhiteSpace(user.CustomerName) || userData.Name == user.CustomerName) &&
                                 (string.IsNullOrWhiteSpace(user.PhoneNumber) || userData.PhoneNumber == user.PhoneNumber)

                                 select new UserAnalysisModel()
                                 {
                                     UserID = userData.UserId,
                                     UserName = userData.Name,
                                     EmailAddress = userData.UserEmail,
                                     PhoneNumber = userData.PhoneNumber,
                                     UniqueUserId = userData.UserUniqueId,
                                     DefaultCurrency = userData.DefaultCurrency != null ? userData.DefaultCurrency.Contains(":") ? userData.DefaultCurrency.Split(':')[1] : userData.DefaultCurrency : null,
                                     IsSocialLogin = userData.IsSocialLogin,
                                     Date = userData.CreatedDate.HasValue ? Convert.ToDateTime(userData.CreatedDate).ToString("dd/MM/yyyy") : string.Empty,
                                     AccountNumber = EncryptDecrypt.Decrypt(userData.PublicKey),
                                     Updateddatetime = Convert.ToDateTime(userData.UpdatedDate),
                                     LastLoginDateTime = Convert.ToDateTime(userData.UpdatedDate).ToString("dd/MM/yyyy")
                                 }).ToList();

                    #region AciveInActiveSection

                    foreach (var item in users)
                    {
                        DateTime today = DateTime.Today;

                        if (item.Updateddatetime.Value.Month == today.Month && item.Updateddatetime.Value.Year == today.Year)
                        {
                            item.IsActive = true;

                        }
                        else
                        {
                            item.IsActive = false;
                        }

                    }

                    #endregion


                    return users;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public bool IsPhoneNumberExists(long id, string phonenumber)
        {
            using (var context = new WYRREntities())
            {
                var userToVerify = context.mstUsers.Where(s => s.PhoneNumber.ToLower().Trim() == phonenumber.ToLower().Trim()).FirstOrDefault();

                if (userToVerify == null)
                {
                    return true;
                }
                else if (userToVerify != null && userToVerify.UserId == id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsUserNameExists(long id, string username)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var userToVerify = context.mstUsers.Where(s => s.Name.ToLower().Trim() == username.ToLower().Trim()).FirstOrDefault();

                    if (userToVerify == null)
                    {
                        return true;
                    }
                    else if (userToVerify != null && userToVerify.UserId == id)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public bool IsEmailAddressExists(long id, string EmailAddress)
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var userToVerify = context.mstUsers.Where(s => s.UserEmail.ToLower().Trim() == EmailAddress.ToLower().Trim()).FirstOrDefault();

                    if (userToVerify == null)
                    {
                        return true;
                    }
                    else if (userToVerify != null && userToVerify.UserId == id)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public int UpdateUserRecordDetails(UserCMSModel userModel)
        {

            using (var context = new WYRREntities())
            {
                int id;

                var userToEdit = context.mstUsers.Where(s => s.UserId == userModel.UserID).SingleOrDefault();
                if (userToEdit != null)
                {

                    //userToEdit.PhoneNumber = userModel.PhoneNumber;
                    userToEdit.Name = userModel.UserName;
                    userToEdit.UserUniqueId = userModel.UniqueUserId;
                    userToEdit.UserEmail = userModel.EmailAddress;
                    userToEdit.DefaultCurrency = userModel.DefaultCurrency;

                    var result = context.SaveChanges();

                    if (result > 0)
                    {
                        id = 1;
                        return id;
                    }

                }
                id = -1;
                return id;

            }


        }

        public int UpdateCMSUserRecordDetails(UserCMSModel userModel)
        {

            using (var context = new WYRREntities())
            {
                int id;

                var userToEdit = context.mstUsers.Where(s => s.UserId == userModel.UserID).SingleOrDefault();
                if (userToEdit != null)
                {
                    //userToEdit.PhoneNumber = userModel.PhoneNumber;
                    userToEdit.Name = userModel.UserName;
                    userToEdit.UserEmail = userModel.EmailAddress;
                    userToEdit.PhoneNumber = userModel.PhoneNumber;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        id = 1;
                        return id;
                    }

                }
                id = -1;
                return id;

            }


        }

        public bool DeleteUserDetails(long id)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToRemove = context.mstUsers.SingleOrDefault(x => x.UserId == id); //returns a single item.

                if (itemToRemove != null)
                {
                    itemToRemove.IsDeleted = true;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }

        public bool ActivateUserDetails(long id)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToRemove = context.mstUsers.SingleOrDefault(x => x.UserId == id); //returns a single item.

                if (itemToRemove != null)
                {
                    itemToRemove.IsDeleted = false;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }

        public bool DeactivateUserDetails(long id)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToRemove = context.mstUsers.SingleOrDefault(x => x.UserId == id); //returns a single item.

                if (itemToRemove != null)
                {
                    itemToRemove.IsDeleted = true;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }

        public bool ResetPassword(ResetPassword passwordmodel)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToEdit = context.mstUsers.SingleOrDefault(x => x.UserId == passwordmodel.UserID); //returns a single item.

                if (itemToEdit != null)
                {
                    itemToEdit.Password = passwordmodel.NewPassword;
                    itemToEdit.ConfirmPassword = passwordmodel.ConfirmPassword;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }

        public int AddCMSUserDetails(UserCMSModel userModel)
        {
            int id;
            using (var context = new WYRREntities())
            {
                mstUser user = new mstUser();
                user.UserEmail = userModel.EmailAddress;
                user.PhoneNumber = userModel.PhoneNumber;
                user.Name = userModel.UserName;
                user.IsAdmin = true;
                user.CreatedDate = DateTime.UtcNow;
                user.UpdatedDate = DateTime.UtcNow;
                user.IsActive = true;
                user.Password = userModel.Password;
                user.ConfirmPassword = userModel.Password;
                user.IsDeleted = false;
                context.mstUsers.Add(user);
                var result = context.SaveChanges();

                if (result > 0)
                {
                    id = 1;
                    return id;
                }
            }
            id = -1;
            return id;

        }

        public List<newsFeedModelCMS> GetNewsFeeds()
        {
            try
            {
                using (var context = new WYRREntities())
                {
                    var feedList = (from feeds in context.newsFeeds.AsEnumerable() // outer sequence
                                    join mu in context.mstUsers.AsEnumerable() //inner sequence 
                                    on feeds.userID equals mu.UserId // key selector )
                                    select new newsFeedModelCMS()
                                    {
                                        User_feedID = feeds.feedID,
                                        UserID = feeds.userID,
                                        UserName = mu.Name,
                                        Title = feeds.title,
                                        Description = feeds.description,
                                        created_at = feeds.created_at.ToString(),
                                        isApproved = feeds.isApproved

                                    }).ToList();
                    return feedList;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public bool ApproveFeed(long feedID)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToApprove = context.newsFeeds.SingleOrDefault(x => x.feedID == feedID); //returns a single item.

                if (itemToApprove != null)
                {
                    itemToApprove.isApproved = true;
                    itemToApprove.modified_by = 2;//Admin User
                    itemToApprove.modified_at = DateTime.UtcNow;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }

        public bool DisapproveFeed(long feedID)
        {
            bool success;

            using (var context = new WYRREntities())
            {

                var itemToDisapprove = context.newsFeeds.SingleOrDefault(x => x.feedID == feedID); //returns a single item.

                if (itemToDisapprove != null)
                {
                    itemToDisapprove.isApproved = false;
                    itemToDisapprove.modified_by = 2;//Admin User
                    itemToDisapprove.modified_at = DateTime.UtcNow;
                    var result = context.SaveChanges();
                    if (result > 0)
                    {
                        success = true;
                        return success;
                    }


                }


            }
            success = false;
            return success;
        }


        #endregion

        #region PrivateMethods

        private bool SendUserEmail(EmailType emailType, mstUser objUserDetails, string TrnAmount)
        {
            try
            {
                SendEmail objEmail = new SendEmail();
                try
                {
                    //decrypt password before sending email to user
                    if( emailType == EmailType.ForgotPassword )
                    {
                        objUserDetails.Password = EncryptDecrypt.Decrypt(objUserDetails.Password);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, Logger.LogType.Error);
                }
                // base.RepositoryContainer.SettingRepository.GetSingle(o => o.Name == "Admin Email");
                return objEmail.SendEmailToUser(emailType, objUserDetails, TrnAmount);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return false;
            }
        }

        public UserContactDetails UpdateUserDetails(UpdateUserModel user)
        {
            UserContactDetails result = new UserContactDetails();
            try
            {
                using (var context = new WYRREntities())
                {
                    var updateUser = context.mstUsers.Where(x => x.UserId == user.UserID).FirstOrDefault();

                    if (updateUser != null)
                    {
                        if (user.ProfileImageUrl != null)
                        {
                            string image = updateUser.UserId + ".jpeg";

                            string ProfileImageURL = ConfigurationManager.AppSettings["ProfileImageURL"];

                            //Get hosting environment path
                            var uploads = HostingEnvironment.MapPath("~/ProfileImages/");

                            //Check if director exists or not
                            if (!Directory.Exists(uploads))
                            {
                                Directory.CreateDirectory(uploads);
                            }

                            byte[] data = Convert.FromBase64String(user.ProfileImageUrl);
                            BinaryWriter writer = new BinaryWriter(File.OpenWrite(uploads + image));
                            writer.Write(data);
                            writer.Flush();
                            writer.Close();

                            string ImageUrl = ProfileImageURL + image;


                            if (updateUser != null)
                            {
                                //result.IsSuccess = true;
                                // result.RecordID = Convert.ToInt32(updateUser.UserId);
                                updateUser.ImageUrl = "ProfileImages/" + image;
                                context.SaveChanges();
                                result.ProfileImageUrl = ProfileImageURL + updateUser.ImageUrl;
                                result.UserID = Convert.ToInt32(updateUser.UserId);
                                result.UserName = updateUser.Name;
                            }

                        }



                    }



                }



            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }

        public UserContactDetails UpdateUserPasswordDetails(UpdateUserPasswordModel user)
        {
            UserContactDetails result = new UserContactDetails();
            try
            {
                using (var context = new WYRREntities())
                {
                    var updateUser = context.mstUsers.Where(x => x.PhoneNumber == user.MobileNumber).FirstOrDefault();

                    if (updateUser != null)
                    {
                        #region ResetPassword 

                        if (user.Password != string.Empty)
                        {
                            if (updateUser.Password.ToLower().Trim() == user.Password.ToLower().Trim())
                            {
                                result.Message = "Please enter different password than previous one.";
                            }
                            else
                            {
                                updateUser.Password = user.Password;
                                updateUser.ConfirmPassword = user.Password;
                                context.SaveChanges();
                                result.UserID = Convert.ToInt32(updateUser.UserId);
                                result.UserName = updateUser.Name;
                                result.ProfileImageUrl = updateUser.ImageUrl;
                                result.Message = "Password updated successfully";
                            }

                        }

                        #endregion





                    }

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return result;
        }

        public string GenerateOTP(long userID)
        {
            int OTPvalue = 0;
            try
            {
                Random rnd = new Random();
                OTPvalue = rnd.Next(1000, 9999);
                using (var context = new WYRREntities())
                {
                    var checkOTP = context.OTPTransactions.Where(x => x.UserId == userID).FirstOrDefault();

                    if (checkOTP != null)
                    {
                        checkOTP.OTP = Convert.ToString(OTPvalue);
                        checkOTP.isVerified = false;
                        checkOTP.VerifiedDateTime = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                    else
                    {
                        OTPTransaction objOTPTransaction = new OTPTransaction();
                        objOTPTransaction.OTP = Convert.ToString(OTPvalue);
                        objOTPTransaction.UserId = userID;
                        objOTPTransaction.isVerified = false;
                        context.OTPTransactions.Add(objOTPTransaction);
                        context.SaveChanges();
                    }




                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return Convert.ToString(OTPvalue);
        }

        public async Task<UserWalletResponseDetails> GetUserWalletDetails(string AccountNumber)
        {
            UserWalletResponseDetails user = new UserWalletResponseDetails();
            try
            {
                #region StellarAPI

                RootObject accountDetails = new RootObject();
                string apiBaseUri = ConfigurationManager.AppSettings["StellarAPIBaseUrl"].ToString();
                var PostClient = new HttpClient();
                PostClient.BaseAddress = new Uri(apiBaseUri);

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("accountId",AccountNumber),

                });

                var response = PostClient.PostAsync(apiBaseUri + "/AccountDetails", formContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    List<Result> result = JsonConvert.DeserializeObject<RootObject>(responseJson).Result;

                    user.IsSuccess = true;
                }
                else
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<ErrorObject>(responseJson).Result;

                    user.Message = result[0].Error;

                }

                #endregion
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return user;

        }

        #endregion


    }
}
