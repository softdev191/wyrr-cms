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
using static PENPALWebAPI.Models.FiatCryptoModel;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Crypto/Withdraw")]
    public class CryptoWithdrawController : ApiController
    {
        private static CultureInfo ci = new CultureInfo("en-US");

        [Route("GetChangellyWithdrawConversion")]
        [HttpPost]
        public ResponseWrapperObject<ChangellyConvRes> GetChangellyWithdrawConversion(ChangellyConvReq convReq)
        {
            ResponseWrapperObject<ChangellyConvRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject convreq = new JObject();
                convreq["from"] = "XLM";
                convreq["to"] = convReq.currency_name;
                convreq["amount"] = convReq.amount;
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails
                    ("getExchangeAmount", convreq);
                //convert name to upper
                ChangellyConvRes chgConvRes = new ChangellyConvRes();
                // List<ChangellyConvRes> lstchgConvRes = new List<ChangellyConvRes>();
                chgConvRes.amount = Convert.ToString(currencyObj.GetValue("result"));
                //lstchgConvRes.Add(chgConvRes);
                chgConvRes.amount = Convert.ToString(currencyObj.GetValue("result"));

                return objResponseWrapper = (chgConvRes.amount != string.Empty ? new ResponseWrapperObject<ChangellyConvRes>(chgConvRes, false, HttpStatusCode.OK, "Success", true, "", null) : new ResponseWrapperObject<ChangellyConvRes>(null, false, HttpStatusCode.OK, "Fail", true, "No Value Received from Changelly", null));
                //return objResponseWrapper = new ResponseWrapperObject<ChangellyConvRes>(chgConvRes, false, HttpStatusCode.OK, "Success", true, "", null);


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<ChangellyConvRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

        [Route("CreateChangellyWithdrawTransaction")]
        [HttpPost]
        public ResponseWrapperObject<ChangellyCreateTxnRes> CreateChangellyWithdrawTransaction(ChangellyCreateTxnReq TxnReq)
        {
            ResponseWrapperObject<ChangellyCreateTxnRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject convreq = new JObject();
                //convreq["from"] = "STR";
                convreq["from"] = "XLM"; //Changed Currency type from STR to XLM on 7.1.2019
                convreq["to"] = TxnReq.currency_name;
                convreq["address"] = TxnReq.walletAddress;
                convreq["amount"] = TxnReq.amount;
                //string RandomValue = GenrateKey(12);
                //convreq["extraId"] = RandomValue;
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails
                    ("createTransaction", convreq);
                if (currencyObj.GetValue("result") != null)
                {
                    //convert name to upper
                    ChangellyCreateTxnRes chgCreateTxn = new ChangellyCreateTxnRes();
                    //List<ChangellyCreateTxnRes> lstchgCreateTxn = new List<ChangellyCreateTxnRes>();
                    chgCreateTxn.depositAddress = Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    //lstchgCreateTxn.Add(chgCreateTxn);


                    UserTransactionModel usrTxnModel = new UserTransactionModel();
                    UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                    //UserCMSModel usrCMSModel = new UserCMSModel();
                    logTransactionDetail objlogTransaction = new logTransactionDetail();
                    mstUser MstUser = new mstUser();
                    using (var context = new WYRREntities())
                    {
                        MstUser = context.mstUsers.Where(x => x.UserId == TxnReq.userID).FirstOrDefault();
                        // reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }
                    // usrCMSModel = usrMgmtProvider.GetUserDetails(Convert.ToInt32(TxnReq.userID));
                    usrTxnModel.Amount = Convert.ToString(TxnReq.amount);
                    usrTxnModel.SenderName = MstUser.Name;
                    usrTxnModel.SenderPublicKey = MstUser.PublicKey;
                    usrTxnModel.SenderSecretKey = MstUser.SecretKey;
                    usrTxnModel.ReceiverPublickKey = EncryptDecrypt.Encrypt(chgCreateTxn.depositAddress);


                    var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                    var task = t.Result;
                    objlogTransaction.TxnID = Convert.ToString(currencyObj.GetValue("result")["id"]);

                    objlogTransaction.TxnType = "Changelly";
                    objlogTransaction.UserID = TxnReq.userID;
                    objlogTransaction.TxnDate = Convert.ToDateTime(currencyObj.GetValue("result")["createdAt"]);
                    objlogTransaction.SenderAddress = EncryptDecrypt.Decrypt(MstUser.PublicKey);//"";// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    objlogTransaction.ReceiverAddress = Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                    objlogTransaction.AmountGiven = TxnReq.amount;
                    // objlogTransaction.CurrencyGiven = "STR";
                    objlogTransaction.CurrencyGiven = "XLM";//Changed Currency type from STR to XLM on 7.1.2019
                    objlogTransaction.AmountReceived = Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                    objlogTransaction.CurrencyReceived = TxnReq.currency_name;
                    objlogTransaction.ChangellyClientMargin = Convert.ToString(currencyObj.GetValue("result")["apiExtraFee"]);
                    objlogTransaction.ChangellyMargin = Convert.ToString(currencyObj.GetValue("result")["changellyFee"]);
                    objlogTransaction.ChangellyClientMarginAmt = 0;
                    objlogTransaction.ChangellyMarginAmt = 0;
                    //objlogTransaction.Memo = RandomValue;
                    LogTransactionProvider logProvider = new LogTransactionProvider();
                    if (task.IsSuccess)
                    {

                        objlogTransaction.TxnStatus = "Approved";
                        logProvider.InsertLogTransaction(objlogTransaction);
                        return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>("Transaction Success", false, HttpStatusCode.OK, "Success", true, "", null);
                    }
                    else
                    {
                        objlogTransaction.TxnStatus = "Failed";
                        logProvider.InsertLogTransaction(objlogTransaction);
                        return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }
                }
                else
                {
                    // Convert.ToString(currencyObj.GetValue("result")["id"])
                    return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(ci.TextInfo.ToTitleCase(Convert.ToString(currencyObj.GetValue("error")["message"])), false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }

            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }

        }


        [Route("CreateChangellyWithdrawTransaction_V2")]
        [HttpPost]
        public ResponseWrapperObject<ChangellyCreateTxnRes_V2> CreateChangellyWithdrawTransaction_V2(ChangellyCreateTxnReq TxnReq)
        {
            ResponseWrapperObject<ChangellyCreateTxnRes_V2> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            List<ChangellyCreateTxnRes_V2> lstResponseModel = new List<ChangellyCreateTxnRes_V2>();

            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject convreq = new JObject();
                convreq["from"] = "XLM";
                convreq["to"] = TxnReq.currency_name;
                convreq["address"] = TxnReq.walletAddress;
                convreq["amount"] = TxnReq.amount;
                string RandomValue = GenrateKey(12);
                convreq["extraId"] = RandomValue;
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails
                    ("createTransaction", convreq);
                if (currencyObj.GetValue("result") != null)
                {
                    //convert name to upper
                    ChangellyCreateTxnRes_V2 chgCreateTxn = new ChangellyCreateTxnRes_V2();
                    //List<ChangellyCreateTxnRes> lstchgCreateTxn = new List<ChangellyCreateTxnRes>();
                    chgCreateTxn.depositAddress = Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    chgCreateTxn.Memo = RandomValue;
                    //lstchgCreateTxn.Add(chgCreateTxn);


                    UserTransactionModel usrTxnModel = new UserTransactionModel();
                    UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                    //UserCMSModel usrCMSModel = new UserCMSModel();
                    logTransactionDetail objlogTransaction = new logTransactionDetail();
                    mstUser MstUser = new mstUser();
                    using (var context = new WYRREntities())
                    {
                        MstUser = context.mstUsers.Where(x => x.UserId == TxnReq.userID).FirstOrDefault();
                        // reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                    }
                    // usrCMSModel = usrMgmtProvider.GetUserDetails(Convert.ToInt32(TxnReq.userID));
                    usrTxnModel.Amount = Convert.ToString(TxnReq.amount);
                    usrTxnModel.SenderName = MstUser.Name;
                    usrTxnModel.SenderPublicKey = MstUser.PublicKey;
                    usrTxnModel.SenderSecretKey = MstUser.SecretKey;
                    usrTxnModel.ReceiverPublickKey = EncryptDecrypt.Encrypt(chgCreateTxn.depositAddress);


                    var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                    var task = t.Result;
                    objlogTransaction.TxnID = Convert.ToString(currencyObj.GetValue("result")["id"]);

                    objlogTransaction.TxnType = "Changelly";
                    objlogTransaction.UserID = TxnReq.userID;
                    objlogTransaction.TxnDate = Convert.ToDateTime(currencyObj.GetValue("result")["createdAt"]);
                    objlogTransaction.SenderAddress = EncryptDecrypt.Decrypt(MstUser.PublicKey);//"";// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    objlogTransaction.ReceiverAddress = Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                    objlogTransaction.AmountGiven = TxnReq.amount;
                    //objlogTransaction.CurrencyGiven = "STR";
                    objlogTransaction.CurrencyGiven = "XLM";//Changed Currency type from STR to XLM on 7.1.2019
                    objlogTransaction.AmountReceived = Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                    objlogTransaction.CurrencyReceived = TxnReq.currency_name;
                    objlogTransaction.ChangellyClientMargin = Convert.ToString(currencyObj.GetValue("result")["apiExtraFee"]);
                    objlogTransaction.ChangellyMargin = Convert.ToString(currencyObj.GetValue("result")["changellyFee"]);
                    objlogTransaction.ChangellyClientMarginAmt = 0;
                    objlogTransaction.ChangellyMarginAmt = 0;
                    objlogTransaction.Memo = RandomValue;
                    LogTransactionProvider logProvider = new LogTransactionProvider();
                    if (task.IsSuccess)
                    {
                        objlogTransaction.TxnStatus = "Approved";
                        logProvider.InsertLogTransaction(objlogTransaction);
                        // Send email functionality
                        UserManagementProvider ObjUser = new UserManagementProvider();
                        try
                        {
                            ResultModel result = ObjUser.SendEmail(MstUser, Common.EmailType.ChangellyTickets, TxnReq.amount.ToString());
                            return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes_V2>(chgCreateTxn, false, HttpStatusCode.OK, "Success", true, "", null);
                        }
                        catch (Exception)
                        {
                            return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes_V2>(chgCreateTxn, false, HttpStatusCode.OK, "Success", true, "", null);
                        }
                    }
                    else
                    {
                        objlogTransaction.TxnStatus = "Failed";
                        logProvider.InsertLogTransaction(objlogTransaction);
                        return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes_V2>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }
                }
                else
                {
                    // Convert.ToString(currencyObj.GetValue("result")["id"])
                    return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes_V2>(ci.TextInfo.ToTitleCase(Convert.ToString(currencyObj.GetValue("error")["message"])), false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }

            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes_V2>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }

        }



        private string GenrateKey(int Length)
        {
            string key = Regex.Replace(Guid.NewGuid().ToString(), "[^0-9]+", "");
            if (key.Length < Length)
            {
                key = key + GenrateKey(Length - key.Length);
            }
            key = key.Substring(0, Length);
            return key;

        }


    }
}