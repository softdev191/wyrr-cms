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

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Crypto/Add")]
    public class CryptoAddController : ApiController
    {
        private static CultureInfo ci = new CultureInfo("en-US");

        [Route("GetChangellyCurrencies")]
        [HttpPost]
        public ResponseWrapper<ChangellyCurrency> GetChangellyCurrencies()
        {
            ResponseWrapper<ChangellyCurrency> objResponseWrapper = null;
           // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails("getCurrenciesFull",new JObject());
                ChangellyCurrency ChangellyCurr = new ChangellyCurrency();
                //convert name to upper
                // var lstChangellyCurrency = currencyObj.GetValue("result").AsEnumerable().ToList().Where(r => r["name"].ToString() != "STR").Select(s => new ChangellyCurrency { currency_name = s["name"].ToString().ToUpper(), currency_fullname = s["fullName"].ToString() }).ToList();
                var lstChangellyCurrency = currencyObj.GetValue("result").AsEnumerable().ToList().Where(r => r["name"].ToString() != "XLM").Select(s => new ChangellyCurrency { currency_name = s["name"].ToString().ToUpper(), currency_fullname = s["fullName"].ToString() }).ToList(); //Changed currency type from STR to XLM on 7.1.2019
                return objResponseWrapper = new ResponseWrapper<ChangellyCurrency>(lstChangellyCurrency, false, HttpStatusCode.OK, "Success", true, "", null);


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapper<ChangellyCurrency>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }

        }

        [Route("GetChangellyConversion")]
        [HttpPost]
        public ResponseWrapperObject<ChangellyConvRes> GetChangellyConversion(ChangellyConvReq convReq)
        {
            ResponseWrapperObject<ChangellyConvRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject convreq = new JObject();
                convreq["from"] = convReq.currency_name;
                convreq["to"] = "XLM";
                convreq["amount"] = convReq.amount;
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails
                    ("getExchangeAmount", convreq);
                //convert name to upper
                ChangellyConvRes chgConvRes = new ChangellyConvRes();
               // List<ChangellyConvRes> lstchgConvRes = new List<ChangellyConvRes>();
                chgConvRes.amount = Convert.ToString(currencyObj.GetValue("result"));
               
                    return objResponseWrapper = (chgConvRes.amount != string.Empty? new ResponseWrapperObject<ChangellyConvRes>(chgConvRes, false, HttpStatusCode.OK, "Success", true, "", null) : new ResponseWrapperObject<ChangellyConvRes>(null, false, HttpStatusCode.OK, "Fail", true, "No Value Received from Changelly", null));
               // lstchgConvRes.Add(chgConvRes);



            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<ChangellyConvRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

        [Route("CreateChangellyTransaction")]
        [HttpPost]
        public ResponseWrapperObject<ChangellyCreateTxnRes> CreateChangellyTransaction(ChangellyCreateTxnReq TxnReq)
        {
            ResponseWrapperObject<ChangellyCreateTxnRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                ChangellyAPI changellyAPI = new ChangellyAPI();
                JObject convreq = new JObject();
                convreq["from"] = TxnReq.currency_name;
                convreq["to"] = "XLM";
                convreq["address"] = TxnReq.walletAddress;
                convreq["amount"] = TxnReq.amount;
                JObject currencyObj = changellyAPI.GetChangellyAPIDetails
                    ("createTransaction", convreq);
                if (currencyObj.GetValue("result") != null)
                {
                    //convert name to upper
                    ChangellyCreateTxnRes chgCreateTxn = new ChangellyCreateTxnRes();
                    //List<ChangellyCreateTxnRes> lstchgCreateTxn = new List<ChangellyCreateTxnRes>();
                    chgCreateTxn.depositAddress = Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    //lstchgCreateTxn.Add(chgCreateTxn);

                    logTransactionDetail objlogTransaction = new logTransactionDetail();
                    objlogTransaction.TxnID = Convert.ToString(currencyObj.GetValue("result")["id"]);
                    objlogTransaction.TxnStatus = "Pending";
                    objlogTransaction.TxnType = "Changelly";
                    objlogTransaction.UserID = TxnReq.userID;
                    objlogTransaction.TxnDate = Convert.ToDateTime(currencyObj.GetValue("result")["createdAt"]);
                    objlogTransaction.SenderAddress = "";//TxnReq.walletAddress;// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    objlogTransaction.ReceiverAddress = Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                    objlogTransaction.AmountGiven = TxnReq.amount;
                    objlogTransaction.CurrencyGiven = TxnReq.currency_name;
                    objlogTransaction.AmountReceived = Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                    //objlogTransaction.CurrencyReceived = "STR";
                    objlogTransaction.CurrencyReceived = "XLM";//Changed currency type from STR to XLM on 7.1.2019
                    objlogTransaction.ChangellyClientMargin = Convert.ToString(currencyObj.GetValue("result")["apiExtraFee"]);
                    objlogTransaction.ChangellyMargin = Convert.ToString(currencyObj.GetValue("result")["changellyFee"]);
                    objlogTransaction.ChangellyClientMarginAmt = 0;
                    objlogTransaction.ChangellyMarginAmt = 0;
                    LogTransactionProvider logProvider = new LogTransactionProvider();

                    logProvider.InsertLogTransaction(objlogTransaction);

                    UserManagementProvider ObjUser = new UserManagementProvider();
                    try
                    {
                        // Send email when Payment Successful
                        mstUser objUser = new mstUser();
                        using (var context = new WYRREntities())
                        {
                            objUser = context.mstUsers.Where(x => x.UserId == TxnReq.userID).FirstOrDefault();
                        }
                        ResultModel result = ObjUser.SendEmail(objUser, Common.EmailType.ChangellyTickets, TxnReq.amount.ToString());
                        return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(chgCreateTxn, false, HttpStatusCode.OK, "Success", true, "", null);
                    }
                    catch (Exception)
                    {
                        return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(chgCreateTxn, false, HttpStatusCode.OK, "Success", true, "", null);
                    }
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(ci.TextInfo.ToTitleCase(Convert.ToString(currencyObj.GetValue("error")["message"])), false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }
            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<ChangellyCreateTxnRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }
    }
}