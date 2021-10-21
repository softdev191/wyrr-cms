using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using PENPAL.DataStore;
using PENPAL.DataStore.APIModel;
using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {
        // GET: Transaction
        /// <summary>
        /// User Wallet Details
        /// </summary>
        /// <param name="wallet"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserWalletBalance")]
        public ResponseWrapper<UserWalletResponseDetails> GetUserWalletBalance(UserWalletModel wallet)
        {
            // Return Object
            ResponseWrapper<UserWalletResponseDetails> objResponseWrapper = null;
            List<UserWalletResponseDetails> lstResponseModel = new List<UserWalletResponseDetails>();
            UserWalletResponseDetails objUserWalletBalance = new UserWalletResponseDetails();

            try
            {
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                //get TransactionHistory
                objUserWalletBalance = ObjUserTransaction.GetUserWalletBalance(wallet);

                //check count
                if (objUserWalletBalance.IsSuccess)
                {
                    objUserWalletBalance.Message = "User Wallet Balance";
                    lstResponseModel.Add(new UserWalletResponseDetails { IsSuccess = objUserWalletBalance.IsSuccess, Balance = objUserWalletBalance.Balance, Message = objUserWalletBalance.Message,LumenCount=objUserWalletBalance.LumenCount, LumenPriceInUSD = objUserWalletBalance.LumenPriceInUSD,PriceInBTC=objUserWalletBalance.PriceInBTC });
                    return objResponseWrapper = new ResponseWrapper<UserWalletResponseDetails>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, objUserWalletBalance.Message, null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserWalletResponseDetails>(null, false, HttpStatusCode.OK, "Fail", true, objUserWalletBalance.Message, null);
                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserWalletResponseDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                objUserWalletBalance = null;
            }

        }

        /// <summary>
        /// User Transaction Summary Details before Making Transaction
        /// </summary>
        /// <param name="wallet"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserDetailsBeforeTransaction")]
        public ResponseWrapper<UserBeforeTransactionResponseDetails> GetUserDetailsBeforeTransaction(UserDetailsBeforeTransaction user)
        {
            // Return Object
            ResponseWrapper<UserBeforeTransactionResponseDetails> objResponseWrapper = null;
            List<UserBeforeTransactionResponseDetails> lstResponseModel = new List<UserBeforeTransactionResponseDetails>();
            UserBeforeTransactionResponseDetails objUserWalletBalance = new UserBeforeTransactionResponseDetails();

            try
            {
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                //get TransactionHistory
                objUserWalletBalance = ObjUserTransaction.GetUserDetailsBeforeTransaction(user);

                //check count
                if (objUserWalletBalance.IsSuccess)
                {

                    lstResponseModel.Add(new UserBeforeTransactionResponseDetails { IsSuccess = objUserWalletBalance.IsSuccess, TotalBalance = objUserWalletBalance.TotalBalance, DeductionAmount = objUserWalletBalance.DeductionAmount, Message = objUserWalletBalance.Message, ReceiverName = objUserWalletBalance.ReceiverName, ServiceTax = objUserWalletBalance.ServiceTax, TaxAmount = objUserWalletBalance.TaxAmount, LumenFee = objUserWalletBalance.LumenFee, FinalLumenDeductionAmount = objUserWalletBalance.FinalLumenDeductionAmount,isServiceTax=objUserWalletBalance.isServiceTax });
                    return objResponseWrapper = new ResponseWrapper<UserBeforeTransactionResponseDetails>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, "User Transaction Summary", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<UserBeforeTransactionResponseDetails>(null, false, HttpStatusCode.OK, "Fail", true, objUserWalletBalance.Message, null);
                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<UserBeforeTransactionResponseDetails>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                objUserWalletBalance = null;
            }

        }


        /// <summary>
        /// User Transaction Details
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SendMoney")]
        public ResponseWrapper<ResponseModel> SendMoney(UserTransactionModel transaction)
        {
            // Return Object
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                mstUser senderUser = new mstUser();
                mstUser reciverUser = new mstUser();
                List<ResponseModel> lstResponseModel = new List<ResponseModel>();

                ResultModel result = new ResultModel();

                #region ValidateUser
                result = ObjUserTransaction.SaveUserTransactionDetails(transaction);
                #endregion


                if (result.IsSuccess)
                {
                    #region ForGettingSenderandReciverMobileNumber

                    using (var context = new WYRREntities())
                    {
                        senderUser = context.mstUsers.Where(x => x.UserId == transaction.SenderUserID).FirstOrDefault();
                        reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }

                    #endregion


                    #region SMSGateway

                    AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                    PublishRequest publishRequest = new PublishRequest();
                    string Currency = transaction.Currency.Contains(":") ? transaction.Currency.Split(':')[1] : transaction.Currency;

                    publishRequest.Message = "You have send " + Currency + transaction.Amount + " to " + reciverUser.Name + " " + "via WYRR";
                    publishRequest.PhoneNumber = "+" + senderUser.CountryCode + senderUser.PhoneNumber;
                    PublishResponse results = smsClient.Publish(publishRequest);

                    PublishRequest publishReciverRequest = new PublishRequest();
                    publishReciverRequest.Message = "You have received " + Currency + transaction.Amount + " from " + senderUser.Name + " " + "via WYRR";
                    publishReciverRequest.PhoneNumber = "+" + reciverUser.CountryCode + reciverUser.PhoneNumber;
                    PublishResponse resultsReciver = smsClient.Publish(publishReciverRequest);


                    #endregion
                    lstResponseModel.Add(new ResponseModel { IsSuccess = true, Message = result.Message });
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, "Transaction done Successfully", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.OK, "Fail", true, result.Message, null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }


        /// <summary>
        /// User Transaction Details
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SendMoney_V2")]
        public ResponseWrapper<ResponseModel> SendMoney_V2(UserTransactionModel transaction)
        {
            // Return Object
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                mstUser senderUser = new mstUser();
                mstUser reciverUser = new mstUser();
                List<ResponseModel> lstResponseModel = new List<ResponseModel>();

                ResultModel result = new ResultModel();

                #region ValidateUser
                result = ObjUserTransaction.SaveUserTransactionDetails_V2(transaction);
                #endregion


                if (result.IsSuccess)
                {
                    #region ForGettingSenderandReciverMobileNumber

                    using (var context = new WYRREntities())
                    {
                        senderUser = context.mstUsers.Where(x => x.UserId == transaction.SenderUserID).FirstOrDefault();
                        reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }

                    #endregion

                    #region SMSGateway

                    AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                    PublishRequest publishRequest = new PublishRequest();
                    string Currency = transaction.Currency.Contains(":") ? transaction.Currency.Split(':')[1] : transaction.Currency;
                    publishRequest.Message = "You have sent " + Currency + transaction.Amount + " to " + reciverUser.Name + " " + "via WYRR";
                    publishRequest.PhoneNumber = "+" + senderUser.CountryCode + senderUser.PhoneNumber;
                    PublishResponse results = smsClient.Publish(publishRequest);

                    PublishRequest publishReciverRequest = new PublishRequest();
                    publishReciverRequest.Message = "You have received " + Currency + transaction.Amount + " from " + senderUser.Name + " " + "via WYRR";
                    publishReciverRequest.PhoneNumber = "+" + reciverUser.CountryCode + reciverUser.PhoneNumber;
                    PublishResponse resultsReciver = smsClient.Publish(publishReciverRequest);


                    #endregion
                    lstResponseModel.Add(new ResponseModel { IsSuccess = true, Message = result.Message });
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, "Transaction done Successfully", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.OK, "Fail", true, result.Message, null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }


        /// <summary>
        /// Withdrawl User Transaction Details
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("WithdrwalMoney")]
        public ResponseWrapper<ResponseModel> WithdrwalMoney(UserTransactionModel transaction)
        {
            // Return Object
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                mstUser senderUser = new mstUser();
                mstUser reciverUser = new mstUser();
                List<ResponseModel> lstResponseModel = new List<ResponseModel>();

                ResultModel result = new ResultModel();

                #region ValidateUser
                result = ObjUserTransaction.WithdrawUserTransactionDetails(transaction);
                #endregion


                if (result.IsSuccess)
                {
                    #region ForGettingSenderandReciverMobileNumber

                    using (var context = new WYRREntities())
                    {
                        senderUser = context.mstUsers.Where(x => x.UserId == transaction.SenderUserID).FirstOrDefault();

                    }

                    #endregion

                    #region SMSGateway

                    AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                    PublishRequest publishRequest = new PublishRequest();
                    string Currency = transaction.Currency.Contains(":") ? transaction.Currency.Split(':')[1] : transaction.Currency;
                    string ReceiverPublickKey = EncryptDecrypt.Decrypt(transaction.ReceiverPublickKey);
                    publishRequest.Message = "You have send " + Currency + transaction.Amount + " to Account Number " + ReceiverPublickKey + " " + "via WYRR";
                    publishRequest.PhoneNumber = "+" + senderUser.CountryCode + senderUser.PhoneNumber;
                    PublishResponse results = smsClient.Publish(publishRequest);


                    #endregion

                    lstResponseModel.Add(new ResponseModel { IsSuccess = true, Message = result.Message });
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, "Transaction done Successfully", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.OK, "Fail", true, result.Message, null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }




        /// <summary>
        /// User TransactionHistory Details
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UserTransactionHistory")]
        public ResponseWrapper<TransactionHistory> GetUserTransactionHistory(UserTransactionHistoryModel user)
        {
            // Return Object
            ResponseWrapper<TransactionHistory> objResponseWrapper = null;
            List<TransactionHistory> lstUserTransactionHistory = new List<TransactionHistory>();

            try
            {
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                //get TransactionHistory
                lstUserTransactionHistory = ObjUserTransaction.GetUserTransactionHistory(user.UserId);

                //check count
                if (lstUserTransactionHistory.Count > 0)
                {
                    return objResponseWrapper = new ResponseWrapper<TransactionHistory>(lstUserTransactionHistory, false, HttpStatusCode.OK, "Success", true, "Transaction History Details", null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<TransactionHistory>(null, false, HttpStatusCode.OK, "Fail", true, "Transaction History Details", null);
                }


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                return objResponseWrapper = new ResponseWrapper<TransactionHistory>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
            finally
            {
                objResponseWrapper = null;
                lstUserTransactionHistory = null;
            }

        }


        /// <summary>
        /// User Transaction Details
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("ReceiveMoney")]
        public ResponseWrapper<ResponseModel> ReceiveMoney(ReceiveMoneyTransactionModel transaction)
        {
            // Return Object
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                List<ResponseModel> lstResponseModel = new List<ResponseModel>();
                mstUser reciverUser = new mstUser();
                ResultModel result = new ResultModel();

                #region ValidateUser
                result = ObjUserTransaction.ReceiveTransactionDetails(transaction);
                #endregion
                if (result.IsSuccess)
                {
                    #region ForGettingSenderandReciverMobileNumber

                    using (var context = new WYRREntities())
                    {
                        reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }

                    #endregion

                    #region SMSGateway

                    AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                    PublishRequest publishRequest = new PublishRequest();
                    string Currency = transaction.Currency.Contains(":") ? transaction.Currency.Split(':')[1] : transaction.Currency;
                    publishRequest.Message = "" + transaction.SenderName + " has requested " + " " + Currency + transaction.Amount + " from you via WYRR.";
                    publishRequest.PhoneNumber = "+" + reciverUser.CountryCode + transaction.ReceiverPhoneNumber;
                    PublishResponse results = smsClient.Publish(publishRequest);


                    #endregion
                    lstResponseModel.Add(new ResponseModel { IsSuccess = true, Message = result.Message });
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, result.Message, null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.OK, "Fail", true, result.Message, null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }


        /// <summary>
        /// User Transaction Details
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("ReceiveMoney_V2")]
        public ResponseWrapper<ResponseModel> ReceiveMoney_V2(ReceiveMoneyTransactionModel_V2 transaction)
        {
            // Return Object
            ResponseWrapper<ResponseModel> objResponseWrapper = null;
            try
            {
                UserTransactionManagementProvider ObjUserTransaction = new UserTransactionManagementProvider();
                string AWSACCESSKEY = ConfigurationManager.AppSettings["AWS_ACCESS_KEY"];
                string AWSSECRETKEY = ConfigurationManager.AppSettings["AWS_SECRET_KEY"];
                List<ResponseModel> lstResponseModel = new List<ResponseModel>();
                mstUser senderUser = new mstUser();
                mstUser reciverUser = new mstUser();
                ResultModel result = new ResultModel();

                #region ValidateUser
                result = ObjUserTransaction.ReceiveTransactionDetails_V2(transaction);
                #endregion


                if (result.IsSuccess)
                {
                    #region ForGettingSenderandReciverMobileNumber

                    using (var context = new WYRREntities())
                    {
                        senderUser = context.mstUsers.Where(x => x.UserId == transaction.SenderUserID).FirstOrDefault();
                        reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }

                    #endregion

                    #region SMSGateway

                    AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(AWSACCESSKEY, AWSSECRETKEY, Amazon.RegionEndpoint.USEast1);

                    PublishRequest publishRequest = new PublishRequest();
                    string Currency = transaction.Currency.Contains(":") ? transaction.Currency.Split(':')[1] : transaction.Currency;
                    publishRequest.Message = "" + transaction.SenderName + " has requested " + " " + Currency + transaction.Amount + " from you via WYRR.";
                    publishRequest.PhoneNumber = "+" + reciverUser.CountryCode + transaction.ReceiverPhoneNumber;
                    PublishResponse results = smsClient.Publish(publishRequest);

                    PublishRequest publishSenderRequest = new PublishRequest();
                    publishSenderRequest.Message = "you have requested " + transaction.Currency + transaction.Amount + " to " + transaction.ReceiverName + " via WYRR.";
                    publishSenderRequest.PhoneNumber = "+" + senderUser.CountryCode + transaction.SenderPhoneNumber;
                    PublishResponse resultsender = smsClient.Publish(publishSenderRequest);


                    #endregion
                    lstResponseModel.Add(new ResponseModel { IsSuccess = true, Message = result.Message });
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(lstResponseModel, false, HttpStatusCode.OK, "Success", true, result.Message, null);
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.OK, "Fail", true, result.Message, null);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                //initialize and return error.
                return objResponseWrapper = new ResponseWrapper<ResponseModel>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }
        }



    }
}