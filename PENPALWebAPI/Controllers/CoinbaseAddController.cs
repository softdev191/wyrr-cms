using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PENPAL.DataStore;
using PENPAL.DataStore.APIModel;
using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using System.Configuration;
using PENPALWebAPI.Models;
using static PENPALWebAPI.Models.FiatCryptoModel;
using Newtonsoft.Json.Linq;
using System.Web.Http;
using Coinbase;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using RestSharp.Authenticators;
using Coinbase.Serialization;
using System.Net.Http.Headers;

namespace PENPALWebAPI.Controllers
{
    [RoutePrefix("api/Coinbase/Add")]
    public class CoinbaseAddController : ApiController
    {

        //5571c640a28ec18cb4487c811c0edae4a7d1757f25a2c7b93878f8c5947ae4fe client ID
        //ee9438703bf2286a60e1f6021bff4ae59f55672e86e6e260a37dd7b38fe753c7
        //cd34af356dd4abc9b96644e844439c98fd4a3c41943b8950ff42e661ae0be073 client secret
        //fc207fafa14905e97617bcd0ab956869c5cc3f834c25d96c474b8b6f35b6a5ac

        CoinbaseAPI cbAPI = new CoinbaseAPI();

        [Route("GetCoinbaseCryptoCurrencies")]
        [HttpPost]
        public ResponseWrapperObject<CoinbaseCurrencies> GetCoinbaseCryptoCurrencies(FiatXLMReq xlmCnt)
        {
            ResponseWrapperObject<CoinbaseCurrencies> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                UserTransactionManagementProvider utm = new UserTransactionManagementProvider();
                // var xlmUSDPrice = utm.GetCurrencyPrice();
                List<JObject> lstObj = new List<JObject>();
                JObject jo = new JObject();
                jo.Add("Name", "Bitcoin");
                jo.Add("Currency", "BTC");
                //jo.Add("Value", Math.Round(xlmCnt.XLMCount * Convert.ToDecimal(utm.GetCurrencyPrice("BTC").Result.LumenInCurrencyAmount), 6));
                JObject jo1 = new JObject();
                jo1.Add("Name", "Litecoin");
                jo1.Add("Currency", "LTC");
                //jo1.Add("Value", Math.Round(xlmCnt.XLMCount * Convert.ToDecimal(utm.GetCurrencyPrice("LTC").Result.LumenInCurrencyAmount), 6));
                JObject jo2 = new JObject();
                jo2.Add("Name", "Ethereum");
                jo2.Add("Currency", "ETH");
                //jo2.Add("Value", Math.Round(xlmCnt.XLMCount * Convert.ToDecimal(utm.GetCurrencyPrice("ETH").Result.LumenInCurrencyAmount), 6));
                JObject jo3 = new JObject();
                jo3.Add("Name", "Bitcoin Cash");
                jo3.Add("Currency", "BCH");
                //jo3.Add("Value", Math.Round(xlmCnt.XLMCount * Convert.ToDecimal(utm.GetCurrencyPrice("BCH").Result.LumenInCurrencyAmount), 6));

                lstObj.Add(jo);
                lstObj.Add(jo1);
                lstObj.Add(jo2);
                lstObj.Add(jo3);
                CoinbaseCurrencies coinbaseCurr = new CoinbaseCurrencies();
                coinbaseCurr.currency = lstObj;
                //convert name to upper
                //var lstChangellyCurrency = currencyObj.GetValue("result").AsEnumerable().ToList().Where(r => r["name"].ToString() != "STR").Select(s => new ChangellyCurrency { currency_name = s["name"].ToString().ToUpper(), currency_fullname = s["fullName"].ToString() }).ToList();

                return objResponseWrapper = new ResponseWrapperObject<CoinbaseCurrencies>(coinbaseCurr, false, HttpStatusCode.OK, "Success", true, "", null);


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<CoinbaseCurrencies>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }

        }


        [Route("GetCurrencyValue")]
        [HttpPost]
        public ResponseWrapperObject<CoinbaseCurrencies> GetCurrencyValue(CoinbaseCurReq xlmCur)
        {
            ResponseWrapperObject<CoinbaseCurrencies> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                UserTransactionManagementProvider utm = new UserTransactionManagementProvider();
                // var xlmUSDPrice = utm.GetCurrencyPrice();
                List<JObject> lstObj = new List<JObject>();
                JObject jo = new JObject();
                jo.Add("Value",Convert.ToString(Math.Round(xlmCur.XLMCount * Convert.ToDecimal(utm.GetCurrencyPrice(xlmCur.Currency).Result.LumenInCurrencyAmount), 6)));
                lstObj.Add(jo);
                CoinbaseCurrencies coinbaseCurr = new CoinbaseCurrencies();
                coinbaseCurr.currency = lstObj;
                //convert name to upper
                //var lstChangellyCurrency = currencyObj.GetValue("result").AsEnumerable().ToList().Where(r => r["name"].ToString() != "STR").Select(s => new ChangellyCurrency { currency_name = s["name"].ToString().ToUpper(), currency_fullname = s["fullName"].ToString() }).ToList();

                return objResponseWrapper = new ResponseWrapperObject<CoinbaseCurrencies>(coinbaseCurr, false, HttpStatusCode.OK, "Success", true, "", null);


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<CoinbaseCurrencies>(null, false, HttpStatusCode.InternalServerError, "Fail", false, ex.Message, null);
            }

        }



        [Route("GetaccessToken")]
        [HttpPost]
        public ResponseWrapperObject<CoinbaseRes> GetaccessToken(CoinbasetokenReq cbReq)
        {
            ResponseWrapperObject<CoinbaseRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                string merchantAddress = cbAPI.GetMerchantAddress();
                UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                //WebClient Client = new WebClient();
                //Client.Headers.Set("Authorization", "Bearer " + cbReq.userToken);
                var jo = cbAPI.GetcoinbaseUserAPIDetails(cbReq.userToken, string.Empty, "GET", "/v2/accounts", null);//Client.DownloadString( + "/v2/accounts");
                string idem = cbReq.userID.ToString() + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
                // JObject jo = JObject.Parse(result);
                if (jo["status"].ToString() == "Success")
                {
                    var results = from x in jo["responseBody"]["data"].Children()
                                  where x["type"].Value<string>() == "wallet"
                                  && x["currency"]["code"].Value<string>() == "BTC"
                                  select x;
                    JObject transferdata = new JObject();
                    //string coinbaseMargin = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.CoinbaseMargin).FirstOrDefault().ToString();
                    transferdata.Add("type", "send");
                    transferdata.Add("to", merchantAddress);
                    transferdata.Add("amount", cbReq.Amt);
                    transferdata.Add("currency", cbReq.Currency);
                    transferdata.Add("idem", idem);
                    var oSendMoney = cbAPI.GetcoinbaseUserAPIDetails(cbReq.userToken, string.Empty, "POST", "/v2/accounts/" + results.First()["id"] + "/transactions", transferdata);//Client.UploadString(apiUrl + "/v2/accounts/" + results.First()["id"] + "/transactions", transferdata.ToString());

                    CoinbaseRes cbRes = new CoinbaseRes();
                    JObject cbResponse = new JObject();
                    cbResponse.Add("amount", cbReq.Amt);
                    cbResponse.Add("currency", cbReq.Currency);
                    cbResponse.Add("idem", idem);
                    cbResponse.Add("userToken", cbReq.userToken);
                    cbRes.response = cbResponse;
                    if (oSendMoney["responseBody"]["id"].ToString() == "two_factor_required")
                    {
                        return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(cbRes, false, HttpStatusCode.OK, "Success", true, "", null);  //JObject oSendMoney = JObject.Parse(CBsendmoney);
                    }
                    else
                    {
                        return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(oSendMoney["responseBody"]["message"], false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(jo["responseBody"]["message"], false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }


            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

        [Route("MakePayment")]
        [HttpPost]
        public ResponseWrapperObject<CoinbaseRes> MakePayment(CoinbasePymtReq cbReq)
        {
            ResponseWrapperObject<CoinbaseRes> objResponseWrapper = null;
            // List<ChangellyCurrency> lstChangellyCurrency = new List<ChangellyCurrency>();
            try
            {
                string merchantAddress = cbAPI.GetMerchantAddress();

                //WebClient Client = new WebClient();
                //Client.Headers.Set("Authorization", "Bearer " + cbReq.userToken);
                var jo = cbAPI.GetcoinbaseUserAPIDetails(cbReq.userToken, string.Empty, "GET", "/v2/accounts", null);//Client.DownloadString( + "/v2/accounts");
                if (jo["status"].ToString() == "Success")
                {
                    // JObject jo = JObject.Parse(result);
                    var results = from x in jo["responseBody"]["data"].Children()
                                  where x["type"].Value<string>() == "wallet"
                                  && x["currency"]["code"].Value<string>() == "BTC"
                                  select x;
                    JObject transferdata = new JObject();
                    transferdata.Add("type", "send");
                    transferdata.Add("to", merchantAddress);
                    transferdata.Add("amount", cbReq.amt);
                    transferdata.Add("currency", cbReq.Currency);
                    transferdata.Add("idem", cbReq.idem);

                    var oSendMoney = cbAPI.GetcoinbaseUserAPIDetails(cbReq.userToken, cbReq.token_2fa, "POST", "/v2/accounts/" + results.First()["id"] + "/transactions", transferdata);//Client.UploadString(apiUrl + "/v2/accounts/" + results.First()["id"] + "/transactions", transferdata.ToString());
                                                                                                                                                                                        //JObject oSendMoney = JObject.Parse(CBsendmoney);
                    if (oSendMoney["status"].ToString() == "success")
                    {
                        UserTransactionModel usrTxnModel = new UserTransactionModel();
                        UserTransactionManagementProvider usrTxnMgmtProvider = new UserTransactionManagementProvider();
                        mstUser MstUser = new mstUser();
                        UserWalletModel usrWallet = new UserWalletModel();
                        UserWalletResponseDetails usrWalletResp = new UserWalletResponseDetails();
                        StellarMasterUser stellarMstUser = new StellarMasterUser();
                        using (var context = new WYRREntities())
                        {
                            MstUser = context.mstUsers.Where(x => x.UserId == cbReq.userID).FirstOrDefault();
                            stellarMstUser = context.StellarMasterUsers.FirstOrDefault();
                        }
                        string curXLMValue = usrTxnMgmtProvider.GetCurrencyPrice(cbReq.Currency).Result.LumenInCurrencyAmount;
                        string coinbaseMargin = usrTxnMgmtProvider.GetTransactionSettingDetails().Select(r => r.CoinbaseMargin).FirstOrDefault().ToString();
                        usrTxnModel.Amount = Convert.ToString(Math.Round(Decimal.Subtract(Convert.ToDecimal(cbReq.amt), Decimal.Multiply(Convert.ToDecimal(cbReq.amt), Decimal.Multiply(Convert.ToInt32(coinbaseMargin), Convert.ToDecimal(0.01)))) / Convert.ToDecimal(curXLMValue), 4));//Convert.ToString(Math.Round(Decimal.Subtract(cbReq.Amt, Decimal.Multiply(cbReq.Amt, Decimal.Multiply(Convert.ToInt32(paypalMargin), Convert.ToDecimal(0.01)))) / Math.Round(txnReq.currentXLMRate, 2), 4));
                        usrTxnModel.SenderName = MstUser.Name;
                        usrTxnModel.SenderPublicKey = stellarMstUser.PublicKey;
                        usrTxnModel.SenderSecretKey = stellarMstUser.SecretKey;
                        usrTxnModel.ReceiverPublickKey = MstUser.PublicKey;
                        usrWallet.AccountNumber = usrTxnModel.SenderPublicKey;
                        usrWalletResp = usrTxnMgmtProvider.GetUserWalletBalance(usrWallet);


                        logTransactionDetail objlogTransaction = new logTransactionDetail();
                        objlogTransaction.TxnID = (oSendMoney["data"].ToList().Count != 0 ? Convert.ToString(oSendMoney["data"]["id"]) : transferdata["idem"].ToString());

                        objlogTransaction.TxnType = "Coinbase";
                        objlogTransaction.UserID = cbReq.userID;
                        objlogTransaction.TxnDate = Convert.ToDateTime(oSendMoney["data"]["created_at"]);
                        objlogTransaction.SenderAddress = results.First()["id"].ToString(); //TxnReq.walletAddress;// Convert.ToString(currencyObj.GetValue("result")["payinAddress"]);
                        objlogTransaction.ReceiverAddress = merchantAddress; //Convert.ToString(oSendMoney["data"]["payoutAddress"]);
                        objlogTransaction.AmountGiven = Convert.ToDecimal(cbReq.amt);
                        objlogTransaction.CurrencyGiven = cbReq.Currency;
                        objlogTransaction.AmountReceived = Convert.ToDecimal(usrTxnModel.Amount);
                        objlogTransaction.CurrencyReceived = "STR";
                        objlogTransaction.CoinbaseMargin = "0";
                        objlogTransaction.CoinbaseClientMargin = coinbaseMargin;
                        objlogTransaction.CoinbaseMarginAmt = 0;
                        objlogTransaction.CoinbaseClientMarginAmt = Decimal.Multiply(Convert.ToDecimal(cbReq.amt), Decimal.Multiply(Convert.ToInt32(coinbaseMargin), Convert.ToDecimal(0.01)));

                        LogTransactionProvider logProvider = new LogTransactionProvider();

                        if (oSendMoney["data"]["status"].ToString() != "pending")
                        {
                            if (Convert.ToDecimal(usrWalletResp.LumenCount) > Convert.ToDecimal(usrTxnModel.Amount))
                            {
                                var t = usrTxnMgmtProvider.SendMoneyTransaction(usrTxnModel);
                                var task = t.Result;

                                if (task.IsSuccess)
                                {
                                    objlogTransaction.TxnStatus = "Approved";
                                    logProvider.InsertLogTransaction(objlogTransaction);
                                    return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>("Payment Successful", false, HttpStatusCode.OK, "Success", true, "", null);
                                }
                                else
                                {
                                    objlogTransaction.TxnStatus = "Failed";
                                    logProvider.InsertLogTransaction(objlogTransaction);
                                    return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(task.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                                }
                            }

                            else
                            {
                                objlogTransaction.TxnStatus = "Failed";
                                logProvider.InsertLogTransaction(objlogTransaction);
                                return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>("No Balance in Merchant Stellar Wallet", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                            }

                        }
                        else
                        {
                            objlogTransaction.TxnStatus = "Pending";
                            logProvider.InsertLogTransaction(objlogTransaction);
                            return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>("Transaction Pending", false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                        }
                    }
                    else
                    {
                        return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(oSendMoney["responseBody"]["message"], false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                    }
                }
                else
                {
                    return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(jo["responseBody"]["message"], false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
                }

            }
            catch (Exception ex)
            {
                return objResponseWrapper = new ResponseWrapperObject<CoinbaseRes>(ex.Message, false, HttpStatusCode.InternalServerError, "Fail", false, "", null);
            }

        }

    }
}