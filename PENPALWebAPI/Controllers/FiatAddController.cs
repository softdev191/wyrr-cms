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
using PayPal;

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Fiat/Add")]
    public class FiatAddController : ApiController
    {
        [Route("GetPayPalXLMRates")]
        [HttpPost]
        public ResponseWrapperObject<FiatXLMRes> GetPayPalXLMRates(FiatXLMReq fiatXLMreq)
        {

            ResponseWrapperObject<FiatXLMRes> objResponseWrapper = null;
            List<FiatXLMRes> lstFiatXLMRes = new List<FiatXLMRes>();
            try
            {
                UserTransactionManagementProvider utm = new UserTransactionManagementProvider();
                var xlmUSDPrice = utm.GetUSDPrice();
                FiatXLMRes fiatRes = new FiatXLMRes();
                fiatRes.XLMDollarRate = Convert.ToDecimal(xlmUSDPrice.Result.LumenInDollarAmount);
                fiatRes.XLMDollarValue = Math.Round(fiatRes.XLMDollarRate * fiatXLMreq.XLMCount, 2);
                //lstFiatXLMRes.Add(fiatRes);
                return objResponseWrapper = new ResponseWrapperObject<FiatXLMRes>(fiatRes, false, HttpStatusCode.OK, "Success", true, "", null);


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<FiatXLMRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

        [Route("MakePayPalPayment")]
        [HttpPost]
        public ResponseWrapperObject<PayPalCreateTxnRes> MakePayPalPayment(PayPalCreateTxnReq txnReq)
        {
            ResponseWrapperObject<PayPalCreateTxnRes> objResponseWrapper = null;
            try
            {
                UserTransactionModel usrTxnModel = new UserTransactionModel();
                UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                logTransactionDetail objlogTransaction = new logTransactionDetail();
                mstUser MstUser = new mstUser();
                UserWalletModel usrWallet = new UserWalletModel();
                UserWalletResponseDetails usrWalletResp = new UserWalletResponseDetails();
                StellarMasterUser stellarMstUser = new StellarMasterUser();
                // PayPal.Api.APIContext apicntxt = PaypalConfig.GetAPIContext();
                // var ppresp= PayPal.Api.Order.Get(apicntxt, txnReq.paypalTxnID);
                using (var context = new WYRREntities())
                {
                    MstUser = context.mstUsers.Where(x => x.UserId == txnReq.userID).FirstOrDefault();
                    stellarMstUser = context.StellarMasterUsers.FirstOrDefault();
                    // reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                }
                string paypalMargin = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.PayPalMargin).FirstOrDefault().ToString();


                usrTxnModel.Amount = Convert.ToString(Math.Round(Decimal.Subtract(txnReq.amount, Decimal.Multiply(txnReq.amount, Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)))) / Math.Round(txnReq.currentXLMRate, 2), 4));


                usrTxnModel.SenderName = MstUser.Name;
                usrTxnModel.SenderPublicKey = stellarMstUser.PublicKey;
                usrTxnModel.SenderSecretKey = stellarMstUser.SecretKey;
                usrTxnModel.ReceiverPublickKey = MstUser.PublicKey;
                usrWallet.AccountNumber = usrTxnModel.SenderPublicKey;
                usrWalletResp = usrTxnMgmtProvider.GetUserWalletBalance(usrWallet);

                objlogTransaction.TxnID = txnReq.paypalTxnID;
                objlogTransaction.TxnType = "PayPal";
                objlogTransaction.UserID = txnReq.userID;
                objlogTransaction.TxnDate = txnReq.paymentTimestamp;
                objlogTransaction.SenderAddress = "Paypal Client";// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                objlogTransaction.ReceiverAddress = "Paypal Merchant";//Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                objlogTransaction.AmountGiven = txnReq.amount;//Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                objlogTransaction.CurrencyGiven = "USD";
                objlogTransaction.AmountReceived = Convert.ToDecimal(usrTxnModel.Amount);
                objlogTransaction.CurrencyReceived = "STR";
                objlogTransaction.PaypalClientMargin = paypalMargin;
                objlogTransaction.PaypalMargin = "0";
                objlogTransaction.PaypalClientMarginAmt = Decimal.Multiply(txnReq.amount, Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)));
                objlogTransaction.PaypalMarginAmt = 0;
                LogTransactionProvider logProvider = new LogTransactionProvider();

                //if (txnReq.paypalResponse == "approved")
                //{
                    if (Convert.ToDecimal(usrWalletResp.LumenCount) > Convert.ToDecimal(usrTxnModel.Amount))
                    {
                        var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                        var task = t.Result;

                        if (task.IsSuccess)
                        {
                            //  logTransactionDetail objlogTransaction = new logTransactionDetail();
                            objlogTransaction.TxnStatus = "Approved";

                            logProvider.InsertLogTransaction(objlogTransaction);

                           // Send email when Payment Successful
                           UserManagementProvider ObjUser = new UserManagementProvider();
                            try
                            {
                            ResultModel result = ObjUser.SendEmail(MstUser, Common.EmailType.PaypalPaymentSuccess, txnReq.amount.ToString());
                            return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Payment Successful", false, HttpStatusCode.OK, "Success", true, "", null);
                            }
                            catch (Exception)
                            {
                              return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Payment Successful", false, HttpStatusCode.OK, "Success", true, "", null);
                            }

                            //
                            //lstFiatXLMRes.Add(fiatRes);
                            
                        }
                        else
                        {
                            objlogTransaction.TxnStatus = "Failed";
                            logProvider.InsertLogTransaction(objlogTransaction);
                            return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                        }
                    }

                    else
                    {
                        objlogTransaction.TxnStatus = "Failed";
                        logProvider.InsertLogTransaction(objlogTransaction);
                        return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("No Balance in Merchant Stellar Wallet", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }
                //}
                //else
                //{
                //    objlogTransaction.TxnStatus = "Failed";
                //    logProvider.InsertLogTransaction(objlogTransaction);
                //    return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Paypal Transaction Failed", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                //}
            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

        [Route("MakePayPalPayment_V2")]
        [HttpPost]
        public ResponseWrapperObject<PayPalCreateTxnRes> MakePayPalPayment_V2(PayPalCreateTxnReq txnReq)
        {
            ResponseWrapperObject<PayPalCreateTxnRes> objResponseWrapper = null;
            try
            {
                UserTransactionModel usrTxnModel = new UserTransactionModel();
                UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                logTransactionDetail objlogTransaction = new logTransactionDetail();
                mstUser MstUser = new mstUser();
                UserWalletModel usrWallet = new UserWalletModel();
                UserWalletResponseDetails usrWalletResp = new UserWalletResponseDetails();
                StellarMasterUser stellarMstUser = new StellarMasterUser();
                // PayPal.Api.APIContext apicntxt = PaypalConfig.GetAPIContext();
                // var ppresp= PayPal.Api.Order.Get(apicntxt, txnReq.paypalTxnID);
                using (var context = new WYRREntities())
                {
                    MstUser = context.mstUsers.Where(x => x.UserId == txnReq.userID).FirstOrDefault();
                    stellarMstUser = context.StellarMasterUsers.FirstOrDefault();
                    // reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                }
                string paypalMargin = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.PayPalMargin).FirstOrDefault().ToString();
                decimal? minimumAmountForPayPalTransaction = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.MinimumAmountForPayPalTransaction).FirstOrDefault();

                if (txnReq.amount > minimumAmountForPayPalTransaction)
                {
                    usrTxnModel.Amount = Convert.ToString(Math.Round(Decimal.Subtract(txnReq.amount, Decimal.Multiply(txnReq.amount, Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)))) / Math.Round(txnReq.currentXLMRate, 2), 4));


                    usrTxnModel.SenderName = MstUser.Name;
                    usrTxnModel.SenderPublicKey = stellarMstUser.PublicKey;
                    usrTxnModel.SenderSecretKey = stellarMstUser.SecretKey;
                    usrTxnModel.ReceiverPublickKey = MstUser.PublicKey;
                    usrWallet.AccountNumber = usrTxnModel.SenderPublicKey;
                    usrWalletResp = usrTxnMgmtProvider.GetUserWalletBalance(usrWallet);

                    objlogTransaction.TxnID = txnReq.paypalTxnID;
                    objlogTransaction.TxnType = "PayPal";
                    objlogTransaction.UserID = txnReq.userID;
                    objlogTransaction.TxnDate = txnReq.paymentTimestamp;
                    objlogTransaction.SenderAddress = "Paypal Client";// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                    objlogTransaction.ReceiverAddress = "Paypal Merchant";//Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                    objlogTransaction.AmountGiven = txnReq.amount;//Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                    objlogTransaction.CurrencyGiven = "USD";
                    objlogTransaction.AmountReceived = Convert.ToDecimal(usrTxnModel.Amount);
                    objlogTransaction.CurrencyReceived = "STR";
                    objlogTransaction.PaypalClientMargin = paypalMargin;
                    objlogTransaction.PaypalMargin = "0";
                    objlogTransaction.PaypalClientMarginAmt = Decimal.Multiply(txnReq.amount, Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)));
                    objlogTransaction.PaypalMarginAmt = 0;
                    LogTransactionProvider logProvider = new LogTransactionProvider();

                    //if (txnReq.paypalResponse == "approved")
                    //{
                        if (Convert.ToDecimal(usrWalletResp.LumenCount) > Convert.ToDecimal(usrTxnModel.Amount))
                        {
                            var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                            var task = t.Result;

                            if (task.IsSuccess)
                            {
                                //  logTransactionDetail objlogTransaction = new logTransactionDetail();
                                objlogTransaction.TxnStatus = "Approved";

                                logProvider.InsertLogTransaction(objlogTransaction);

                                //
                                //lstFiatXLMRes.Add(fiatRes);
                                return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Payment Successful", false, HttpStatusCode.OK, "Success", true, "", null);
                            }
                            else
                            {
                                objlogTransaction.TxnStatus = "Failed";
                                logProvider.InsertLogTransaction(objlogTransaction);
                                return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                            }
                        }

                        else
                        {
                            objlogTransaction.TxnStatus = "Failed";
                            logProvider.InsertLogTransaction(objlogTransaction);
                            return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("No Balance in Merchant Stellar Wallet", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                        }
                    //}
                    //else
                    //{
                    //    objlogTransaction.TxnStatus = "Failed";
                    //    logProvider.InsertLogTransaction(objlogTransaction);
                    //    return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Paypal Transaction Failed", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    //}


                }
                else
                {
                    return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Please enter minimum amount for transfer ($" + minimumAmountForPayPalTransaction + ")", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }

            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }


        [Route("GetMinimumPaypalAmount")]
        [HttpGet]
        public ResponseWrapperObject<PayPalMinimumAmount> GetMinimumPaypalAmount()
        {
            ResponseWrapperObject<PayPalMinimumAmount> objResponseWrapper = null;
            try
            {
                UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                decimal? minimumAmountForPayPalTransaction = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.MinimumAmountForPayPalTransaction).FirstOrDefault();
                PayPalMinimumAmount chgCreateTxn = new PayPalMinimumAmount();
                chgCreateTxn.MiniumPayPalAmountinDollar = Convert.ToString(minimumAmountForPayPalTransaction);
                return objResponseWrapper = new ResponseWrapperObject<PayPalMinimumAmount>(chgCreateTxn, false, HttpStatusCode.OK, "Success", true,"", null);

            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<PayPalMinimumAmount>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }


    }
}