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

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Fiat/Withdraw")]
    public class FiatWithdrawController : ApiController
    {

        [Route("WithdrawPayPalPayment")]
        [HttpPost]
        public ResponseWrapperObject<PayPalCreateTxnRes> WithdrawPayPalPayment(PayPalCreateTxnReq txnReq)
        {
            ResponseWrapperObject<PayPalCreateTxnRes> objResponseWrapper = null;
            try
            {

                UserTransactionModel usrTxnModel = new UserTransactionModel();
                UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                //UserCMSModel usrCMSModel = new UserCMSModel();
                logTransactionDetail objlogTransaction = new logTransactionDetail();
                mstUser MstUser = new mstUser();
                StellarMasterUser stellarMstUser = new StellarMasterUser();
                using (var context = new WYRREntities())
                {
                    MstUser = context.mstUsers.Where(x => x.UserId == txnReq.userID).FirstOrDefault();
                    stellarMstUser = context.StellarMasterUsers.FirstOrDefault();
                    // reciverUser = context.mstUsers.Where(x => x.UserId == transaction.ReceiverUserID).FirstOrDefault();
                }
                // usrCMSModel = usrMgmtProvider.GetUserDetails(Convert.ToInt32(TxnReq.userID));
                usrTxnModel.Amount = Convert.ToString(txnReq.amount);
                usrTxnModel.SenderName = MstUser.Name;
                usrTxnModel.SenderPublicKey = MstUser.PublicKey;
                usrTxnModel.SenderSecretKey = MstUser.SecretKey;
                usrTxnModel.ReceiverPublickKey = stellarMstUser.PublicKey;
                string paypalMargin = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.PayPalMargin).FirstOrDefault().ToString();

                Decimal amtrec = Math.Round(Decimal.Subtract(Decimal.Multiply(txnReq.amount, Math.Round(txnReq.currentXLMRate, 2)), Decimal.Multiply(Decimal.Multiply(txnReq.amount, Math.Round(txnReq.currentXLMRate, 2)), Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)))), 4);

                objlogTransaction.TxnType = "PayPal";
                objlogTransaction.UserID = txnReq.userID;
                objlogTransaction.TxnDate = txnReq.paymentTimestamp;
                objlogTransaction.SenderAddress = "Paypal Client";// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                objlogTransaction.ReceiverAddress = "Paypal Merchant";//Convert.ToString(currencyObj.GetValue("result")["payoutAddress"]);
                objlogTransaction.AmountGiven = txnReq.amount;//Convert.ToDecimal(currencyObj.GetValue("result")["amountTo"]);
                objlogTransaction.CurrencyGiven = "STR";
                objlogTransaction.AmountReceived = amtrec;//Decimal.Subtract(Decimal.Multiply(txnReq.amount, txnReq.currentXLMRate), Decimal.Multiply(Decimal.Multiply(txnReq.amount, txnReq.currentXLMRate), Decimal.Multiply(Convert.ToInt32(ConfigurationManager.AppSettings["PaypalMargin"]), Convert.ToDecimal(0.01))));//(txnReq.amount- / txnReq.currentXLMRate;
                objlogTransaction.CurrencyReceived = "USD";
                objlogTransaction.PaypalClientMargin = paypalMargin;
                objlogTransaction.PaypalMargin = "0";
                objlogTransaction.PaypalClientMarginAmt = Decimal.Multiply(Decimal.Multiply(txnReq.amount, Math.Round(txnReq.currentXLMRate, 2)), Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)));
                objlogTransaction.PaypalMarginAmt = 0;
                LogTransactionProvider logProvider = new LogTransactionProvider();

                var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                var task = t.Result;
                if (task.IsSuccess)
                {
                    //Paypal Payment
                    PayPal.Api.APIContext apicntxt = PaypalConfig.GetAPIContext();
                    PayPal.Api.Payout po = new PayPal.Api.Payout();
                    string txn_id = txnReq.userID.ToString() + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
                    PayPal.Api.PayoutItem poi = new PayPal.Api.PayoutItem();
                    PayPal.Api.PayoutSenderBatchHeader pob = new PayPal.Api.PayoutSenderBatchHeader();
                    pob.sender_batch_id = txn_id;
                    pob.email_subject = "Payment Successful";
                    poi.recipient_type = PayPal.Api.PayoutRecipientType.EMAIL;
                    PayPal.Api.Currency cu = new PayPal.Api.Currency();
                    cu.value = Convert.ToString(Math.Round(amtrec, 2));
                    cu.currency = "USD";
                    poi.amount = cu;
                    //poi.amount.currency= "USD";
                    poi.receiver = txnReq.paypalTxnID;
                    poi.note = "Payment against XLM";
                    poi.sender_item_id = txn_id + "01";
                    po.items = new List<PayPal.Api.PayoutItem>(1);
                    po.items.Add(poi);
                    po.sender_batch_header = pob;


                    objlogTransaction.TxnID = txn_id;
                    //PayPal.Api.Payment pymt = new PayPal.Api.Payment();
                    //PayPal.Api.Payer pyr = new PayPal.Api.Payer();
                    //PayPal.Api.RedirectUrls pyrredirect = new PayPal.Api.RedirectUrls();
                    //PayPal.Api.Transaction pyrtxn = new PayPal.Api.Transaction();
                    //List<PayPal.Api.Transaction> pyrlsttxn = new List<PayPal.Api.Transaction>();
                    //PayPal.Api.Amount pyramt = new PayPal.Api.Amount();
                    //pyr.payment_method = "paypal";
                    //pymt.intent = "sale";
                    //pyrredirect.cancel_url = pyrredirect.return_url = ConfigurationManager.AppSettings["PenpalAPIBaseUrl"] + "api/fiat/Withdraw/WithdrawPayPalPayment";
                    //pyramt.total = Convert.ToString(Math.Round(amtrec, 2));
                    //pyramt.currency = "USD";
                    //pyrtxn.amount = pyramt;
                    //// pymt.redirect_urls=
                    //pymt.payer = pyr;
                    //pymt.redirect_urls = pyrredirect;
                    //pyrlsttxn.Add(pyrtxn);
                    //pymt.transactions = pyrlsttxn;
                    try
                    {
                        var po_resp = po.Create(apicntxt);
                        //var ppresp = PayPal.Api.Payment.Create(apicntxt, pymt);
                        //PayPal.Api.PaymentExecution pyrExc = new PayPal.Api.PaymentExecution();
                        //pyrExc.payer_id = txnReq.paypalTxnID;
                        //var paypalTxn = PayPal.Api.Payment.Execute(apicntxt, ppresp.id, pyrExc);
                        //if (po_resp.batch_header.batch_status == "approved")
                        //{
                        objlogTransaction.TxnStatus = "approved";//po_resp.batch_header.batch_status;
                        logProvider.InsertLogTransaction(objlogTransaction);
                        return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Withdrawal Successful", false, HttpStatusCode.OK, "Success", true, "", null);

                        //}
                        //else
                        //{
                        //    objlogTransaction.TxnStatus = "Failed";
                        //    logProvider.InsertLogTransaction(objlogTransaction);
                        //    return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>("Paypal Transaction Failed", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                        //}
                    }
                    catch (PayPal.PaymentsException ex)
                    {
                        return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(ex.Details.message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }

                }
                else
                {
                    objlogTransaction.TxnStatus = "Failed";
                    logProvider.InsertLogTransaction(objlogTransaction);
                    return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }


                //
                //lstFiatXLMRes.Add(fiatRes);



            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<PayPalCreateTxnRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }
    }
}