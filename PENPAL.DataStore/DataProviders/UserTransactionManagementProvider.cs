using Newtonsoft.Json;
using PENPAL.DataStore.StellarModel;
using PENPAL.DataStore.APIModel;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static PENPAL.DataStore.StellarModel.StellarWalletModel;
using static PENPAL.DataStore.StellarModel.StellarAPIErrorModel;
using static PENPAL.DataStore.StellarModel.SendMoneyModel;
using System.Web.Configuration;
using PENPAL.DataStore.SMSHelper;
using PENPAL.DataStore.Templates;
using static PENPAL.DataStore.StellarModel.StellarLumenConversion;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace PENPAL.DataStore.DataProviders
{
    public class UserTransactionManagementProvider
    {
        public static WYRREntities db = new WYRREntities();

        public ResultModel SaveUserTransactionDetails(UserTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {

                #region ValidationSetting

                using (var context = new WYRREntities())
                {

                    decimal? minimumAmount = context.TransactionSettings.AsEnumerable().Select(x => x.MinimumAmountForTransaction).FirstOrDefault();

                    decimal? checkAmount = Convert.ToDecimal(objUserTransaction.Amount);

                    if (checkAmount > minimumAmount)
                    {

                        #region DollartoLumen

                        var price = GetUSDPrice();

                        var getPrice = price.Result;

                        string LumenInDollarValue = getPrice.LumenInDollarAmount;

                        ////we are getting balance in dollar we have to convert in lumen 
                        double balance = Convert.ToDouble(objUserTransaction.Amount) / Convert.ToDouble(LumenInDollarValue);
                        balance = Math.Round(balance, 2);
                        String Amount = Convert.ToString(balance);
                        objUserTransaction.Amount = Amount;

                        #endregion

                        var t = SendMoneyTransaction(objUserTransaction);
                        var task = t.Result;

                        if (task.IsSuccess)
                        {
                            #region LumentoDollar

                            //we are getting balance in lumen we have to convert in usd 
                            double balances = Convert.ToDouble(objUserTransaction.Amount) * Convert.ToDouble(LumenInDollarValue);
                            objUserTransaction.Amount = Convert.ToString(balances);

                            #endregion

                            #region SaveTransaction

                            trnUserTransactionDetail objUserTransactionDetail = new trnUserTransactionDetail();

                            objUserTransactionDetail.SenderUserId = objUserTransaction.SenderUserID;
                            objUserTransactionDetail.SenderPublicKey = objUserTransaction.SenderPublicKey;
                            objUserTransactionDetail.SenderSecretKey = objUserTransaction.SenderSecretKey;
                            objUserTransactionDetail.ReceiverUserId = objUserTransaction.ReceiverUserID;
                            objUserTransactionDetail.ReceiverPublicKey = objUserTransaction.ReceiverPublickKey;
                            objUserTransactionDetail.ISTXNVERIFIED = true;
                            objUserTransactionDetail.TXNDATE = DateTime.UtcNow;
                            objUserTransactionDetail.Amount = objUserTransaction.Amount;
                            objUserTransactionDetail.Currency = objUserTransaction.Currency;
                            objUserTransactionDetail.Status = task.Message;
                            context.trnUserTransactionDetails.Add(objUserTransactionDetail);
                            context.SaveChanges();




                            #endregion

                            result.IsSuccess = true;
                            result.Message = task.Message;

                        }
                        else
                        {
                            result.Message = task.Message;
                        }

                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Please enter minimum amount for transfer ($" + minimumAmount + ")";
                    }


                }


                #endregion
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }

        public ResultModel SaveUserTransactionDetails_V2(UserTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {
                using (var context = new WYRREntities())
                {

                    decimal? minimumAmount = context.TransactionSettings.AsEnumerable().Select(x => x.MinimumAmountForTransaction).FirstOrDefault();

                    decimal? checkAmount = Convert.ToDecimal(objUserTransaction.Amount);

                    double LumenFeesInDollar = 0;
                    double Lumenfees = 0;


                    #region GlobalTransactionAmountValidation

                    if (checkAmount > minimumAmount)
                    {

                        #region UserTransactionPerDayValidation

                        long? userTransactionPerDay = context.trnUserTransactionSettings.AsEnumerable().Where(x => x.UserId == objUserTransaction.SenderUserID).Select(x => x.TransactionPerDay).FirstOrDefault();

                        var checkUserTransactionPerDayList = context.trnUserTransactionDetails.AsEnumerable().Where(x => x.SenderUserId == objUserTransaction.SenderUserID && x.TXNDATE.Value.Date == DateTime.UtcNow.Date).ToList();


                        long checkUserTransactionPerDay = checkUserTransactionPerDayList.Count();

                        if (checkUserTransactionPerDay < userTransactionPerDay)
                        {
                            #region UserTransactionAmountPerDayValidation

                            decimal? userTransactionAmountPerDay = context.trnUserTransactionSettings.AsEnumerable().Where(x => x.UserId == objUserTransaction.SenderUserID).Select(x => x.TransactionAmountPerDay).FirstOrDefault();

                            var checkUserTransactionAmountPerDayList = context.trnUserTransactionDetails.AsEnumerable().Where(x => x.SenderUserId == objUserTransaction.SenderUserID && x.TXNDATE.Value.Date == DateTime.UtcNow.Date).ToList();

                            decimal? userTransactionAmount = checkUserTransactionAmountPerDayList.Sum(x => Convert.ToDecimal(x.Amount));

                            #region OLDAmountCalculationCode

                            //var checkUserTransactionAmountPerDayList = context.trnUserTransactionDetails.AsEnumerable().Where(x => x.SenderUserId == objUserTransaction.SenderUserID && x.TXNDATE.Value.Date == DateTime.UtcNow.Date).Select(x => x.Amount).ToList();
                            //decimal? userTransactionAmount = 0;

                            //foreach (var item in checkUserTransactionAmountPerDayList)
                            //{
                            //    userTransactionAmount += Convert.ToDecimal(item);
                            //}

                            #endregion

                            if (userTransactionAmount < userTransactionAmountPerDay)
                            {

                                bool checkServiceTaxSetting = context.TransactionSettings.Select(x => x.IsActive).SingleOrDefault().Value;

                                if (checkServiceTaxSetting)
                                {
                                    #region ServiceTaxDeduction

                                    var servicetax = context.TransactionSettings.Select(x => x.ServiceTaxPerTransaction).FirstOrDefault();

                                    var servicetaxwithoutpercentage = servicetax.Replace("%", "");

                                    //Tax Amount In Dollar
                                    double TaxAmount = Convert.ToDouble(objUserTransaction.Amount) * (Convert.ToDouble(servicetaxwithoutpercentage) / 100);

                                    objUserTransaction.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - TaxAmount);

                                    //In Dollar
                                    objUserTransaction.TaxAmount = TaxAmount;

                                    #endregion
                                }
                                else
                                {
                                    #region LumenDeduction

                                    //reduce that much lumen from user balance
                                    var TransactionRangeChargesList = context.TransactionRangeCharges.ToList();

                                    //convert dollar amount into lumen amount for comparison
                                    var prices = GetUSDPrice();

                                    var getPrices = prices.Result;

                                    //get one lumen in dollar value
                                    string LumenInDollarValues = getPrices.LumenInDollarAmount;

                                    ////we are getting balance in dollar we have to convert in lumen 
                                    double balances = Convert.ToDouble(objUserTransaction.Amount) / Convert.ToDouble(LumenInDollarValues);


                                    #region SlabComparison

                                    //check amount as per deduct lumen
                                    foreach (var item in TransactionRangeChargesList)
                                    {

                                        if (item.min < balances && item.max >= balances)
                                        {
                                            Lumenfees = Convert.ToDouble(item.Fee_Lumen_);
                                        }

                                    }

                                    #endregion

                                    //convert lumen fees into dollar
                                    LumenFeesInDollar = Lumenfees * Convert.ToDouble(LumenInDollarValues);

                                    objUserTransaction.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - LumenFeesInDollar);

                                    //In Dollar
                                    objUserTransaction.TaxAmount = LumenFeesInDollar;

                                    #endregion

                                }

                                #region DollartoLumen

                                var price = GetUSDPrice();

                                var getPrice = price.Result;

                                string LumenInDollarValue = getPrice.LumenInDollarAmount;

                                ////we are getting balance in dollar we have to convert in lumen 
                                double balance = Convert.ToDouble(objUserTransaction.Amount) / Convert.ToDouble(LumenInDollarValue);
                                balance = Math.Round(balance, 2);
                                String Amount = Convert.ToString(balance);
                                objUserTransaction.Amount = Amount;

                                #endregion

                                #region TaxAmountInLumen

                                ////we are getting tax amount in dollar we have to convert in lumen 
                                double TaxAmountbalance = Convert.ToDouble(objUserTransaction.TaxAmount) / Convert.ToDouble(LumenInDollarValue);
                                TaxAmountbalance = Math.Round(TaxAmountbalance, 2);

                                objUserTransaction.TaxAmount = TaxAmountbalance;



                                #endregion

                                #region TransferMoneytoReceiver

                                var SendMoney = SendMoneyTransaction(objUserTransaction);
                                var task = SendMoney.Result;

                                #endregion

                                if (task.IsSuccess)
                                {
                                    #region TransferMoneytoMasterAccount

                                    var SendTaxAmountToMaster = SendMoneyToMasterAccount(objUserTransaction);

                                    var checkTask = SendTaxAmountToMaster.Result;

                                    #endregion

                                    #region LumentoDollar

                                    ////we are getting balance in lumen we have to convert in usd 
                                    double balances = Convert.ToDouble(objUserTransaction.Amount) * Convert.ToDouble(LumenInDollarValue);
                                    objUserTransaction.Amount = Convert.ToString(balances);

                                    #endregion

                                    #region SaveTransaction

                                    trnUserTransactionDetail objUserTransactionDetail = new trnUserTransactionDetail();

                                    objUserTransactionDetail.SenderUserId = objUserTransaction.SenderUserID;
                                    objUserTransactionDetail.SenderPublicKey = objUserTransaction.SenderPublicKey;
                                    objUserTransactionDetail.SenderSecretKey = objUserTransaction.SenderSecretKey;
                                    objUserTransactionDetail.ReceiverUserId = objUserTransaction.ReceiverUserID;
                                    objUserTransactionDetail.ReceiverPublicKey = objUserTransaction.ReceiverPublickKey;
                                    objUserTransactionDetail.ISTXNVERIFIED = true;
                                    objUserTransactionDetail.TXNDATE = DateTime.UtcNow;
                                    objUserTransactionDetail.Amount = objUserTransaction.Amount;
                                    //objUserTransactionDetail.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - TaxAmount);
                                    objUserTransactionDetail.Currency = objUserTransaction.Currency;
                                    objUserTransactionDetail.Status = task.Message;
                                    context.trnUserTransactionDetails.Add(objUserTransactionDetail);
                                    context.SaveChanges();




                                    #endregion

                                    result.IsSuccess = true;
                                    result.Message = task.Message;

                                }
                                else
                                {
                                    result.Message = task.Message;
                                }


                            }
                            else
                            {
                                result.Message = "Daily Amount Limit has been exceeded";
                                result.IsSuccess = false;
                            }

                            #endregion

                        }
                        else
                        {
                            result.Message = "Daily Transaction Limit has been exceeded";
                            result.IsSuccess = false;
                        }

                        #endregion

                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Please enter minimum amount for transfer ($" + minimumAmount + ")";
                    }


                    #endregion

                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }

        public ResultModel WithdrawUserTransactionDetails(UserTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {
                using (var context = new WYRREntities())
                {

                    decimal? minimumAmount = context.TransactionSettings.AsEnumerable().Select(x => x.MinimumAmountForTransaction).FirstOrDefault();

                    decimal? checkAmount = Convert.ToDecimal(objUserTransaction.Amount);

                    double LumenFeesInDollar = 0;
                    double Lumenfees = 0;

                    #region GlobalTransactionAmountValidation

                    if (checkAmount > minimumAmount)
                    {

                        #region UserTransactionPerDayValidation

                        long? userTransactionPerDay = context.trnUserTransactionSettings.AsEnumerable().Where(x => x.UserId == objUserTransaction.SenderUserID).Select(x => x.TransactionPerDay).FirstOrDefault();

                        var checkUserTransactionPerDayList = context.trnUserTransactionDetails.AsEnumerable().Where(x => x.SenderUserId == objUserTransaction.SenderUserID && x.TXNDATE.Value.Date == DateTime.UtcNow.Date).ToList();


                        long checkUserTransactionPerDay = checkUserTransactionPerDayList.Count();

                        if (checkUserTransactionPerDay < userTransactionPerDay)
                        {
                            #region UserTransactionAmountPerDayValidation

                            decimal? userTransactionAmountPerDay = context.trnUserTransactionSettings.AsEnumerable().Where(x => x.UserId == objUserTransaction.SenderUserID).Select(x => x.TransactionAmountPerDay).FirstOrDefault();

                            var checkUserTransactionAmountPerDayList = context.trnUserTransactionDetails.AsEnumerable().Where(x => x.SenderUserId == objUserTransaction.SenderUserID && x.TXNDATE.Value.Date == DateTime.UtcNow.Date).ToList();

                            decimal? userTransactionAmount = checkUserTransactionAmountPerDayList.Sum(x => Convert.ToDecimal(x.Amount));

                            if (userTransactionAmount < userTransactionAmountPerDay)
                            {
                                bool checkServiceTaxSetting = context.TransactionSettings.Select(x => x.IsActive).SingleOrDefault().Value;

                                if (checkServiceTaxSetting)
                                {
                                    #region ServiceTaxDeduction

                                    var servicetax = context.TransactionSettings.Select(x => x.ServiceTaxPerTransaction).FirstOrDefault();

                                    var servicetaxwithoutpercentage = servicetax.Replace("%", "");

                                    double TaxAmount = Convert.ToDouble(objUserTransaction.Amount) * (Convert.ToDouble(servicetaxwithoutpercentage) / 100);

                                    objUserTransaction.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - TaxAmount);

                                    #endregion
                                }
                                else
                                {
                                    #region LumenDeduction

                                    //reduce that much lumen from user balance
                                    var TransactionRangeChargesList = context.TransactionRangeCharges.ToList();

                                    //long? TransactionAmount = Convert.ToInt64(objUserTransaction.Amount);

                                    //convert dollar amount into lumen amount for comparison
                                    var prices = GetUSDPrice();

                                    var getPrices = prices.Result;

                                    //get one lumen in dollar value
                                    string LumenInDollarValues = getPrices.LumenInDollarAmount;

                                    ////we are getting balance in dollar we have to convert in lumen 
                                    double balances = Convert.ToDouble(objUserTransaction.Amount) / Convert.ToDouble(LumenInDollarValues);


                                    #region SlabComparison

                                    //check amount as per deduct lumen
                                    foreach (var item in TransactionRangeChargesList)
                                    {

                                        if (item.min < balances && item.max >= balances)
                                        {
                                            Lumenfees = Convert.ToDouble(item.Fee_Lumen_);
                                        }

                                    }

                                    #endregion

                                    //convert lumen fees into dollar
                                    LumenFeesInDollar = Lumenfees * Convert.ToDouble(LumenInDollarValues);

                                    objUserTransaction.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - LumenFeesInDollar);

                                    #endregion
                                }

                                #region DollartoLumen

                                var price = GetUSDPrice();

                                var getPrice = price.Result;

                                string LumenInDollarValue = getPrice.LumenInDollarAmount;



                                ////we are getting balance in dollar we have to convert in lumen 
                                double balance = Convert.ToDouble(objUserTransaction.Amount) / Convert.ToDouble(LumenInDollarValue);
                                balance = Math.Round(balance, 2);
                                String Amount = Convert.ToString(balance);
                                objUserTransaction.Amount = Amount;

                                #endregion

                                #region TaxAmountInLumen

                                ////we are getting tax amount in dollar we have to convert in lumen 
                                double TaxAmountbalance = Convert.ToDouble(objUserTransaction.TaxAmount) / Convert.ToDouble(LumenInDollarValue);
                                TaxAmountbalance = Math.Round(TaxAmountbalance, 2);

                                objUserTransaction.TaxAmount = TaxAmountbalance;



                                #endregion


                                var t = SendMoneyTransaction(objUserTransaction);
                                var task = t.Result;

                                if (task.IsSuccess)
                                {
                                    #region TransferMoneytoMasterAccount

                                    var SendTaxAmountToMaster = SendMoneyToMasterAccount(objUserTransaction);

                                    var checkTask = SendTaxAmountToMaster.Result;

                                    #endregion

                                    #region LumentoDollar

                                    ////we are getting balance in lumen we have to convert in usd 
                                    double balances = Convert.ToDouble(objUserTransaction.Amount) * Convert.ToDouble(LumenInDollarValue);
                                    objUserTransaction.Amount = Convert.ToString(balances);

                                    #endregion

                                    #region SaveTransaction

                                    trnUserTransactionDetail objUserTransactionDetail = new trnUserTransactionDetail();

                                    objUserTransactionDetail.SenderUserId = objUserTransaction.SenderUserID;
                                    objUserTransactionDetail.SenderPublicKey = objUserTransaction.SenderPublicKey;
                                    objUserTransactionDetail.SenderSecretKey = objUserTransaction.SenderSecretKey;
                                    objUserTransactionDetail.ReceiverUserId = objUserTransaction.ReceiverUserID;
                                    objUserTransactionDetail.ReceiverPublicKey = objUserTransaction.ReceiverPublickKey;
                                    objUserTransactionDetail.ISTXNVERIFIED = true;
                                    objUserTransactionDetail.TXNDATE = DateTime.UtcNow;
                                    objUserTransactionDetail.Amount = objUserTransaction.Amount;
                                    //objUserTransactionDetail.Amount = Convert.ToString(Convert.ToDouble(objUserTransaction.Amount) - TaxAmount);
                                    objUserTransactionDetail.Currency = objUserTransaction.Currency;
                                    objUserTransactionDetail.Status = task.Message;
                                    context.trnUserTransactionDetails.Add(objUserTransactionDetail);
                                    context.SaveChanges();




                                    #endregion

                                    result.IsSuccess = true;
                                    result.Message = task.Message;

                                }
                                else
                                {
                                    result.Message = task.Message;
                                }


                            }
                            else
                            {
                                result.Message = "Daily Amount Limit has been exceeded";
                                result.IsSuccess = false;
                            }

                            #endregion

                        }
                        else
                        {
                            result.Message = "Daily Transaction Limit has been exceeded";
                            result.IsSuccess = false;
                        }

                        #endregion

                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Please enter minimum amount for transfer ($" + minimumAmount + ")";
                    }


                    #endregion

                }

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }


        public List<TransactionHistory> GetUserTransactionHistory(int UserId)
        {
            List<TransactionHistory> lstUserTransactionHistory = new List<TransactionHistory>();
            try
            {

                using (var context = new WYRREntities())
                {
                    //get TransactionHistory from userid
                    var lstUserTransHistory = context.GetUserTransactionHistory(UserId).ToList();

                    // var price = GetUSDPrice();

                    // var getPrice = price.Result;

                    // string LumenInDollarValue = getPrice.LumenInDollarAmount;

                    #region AddTransactionHistoryinModel

                    foreach (var item in lstUserTransHistory)
                    {
                        TransactionHistory objTransaction = new TransactionHistory();

                        string date = (Convert.ToDateTime(item.TXNDATE)).ToString("dd/MM/yyyy");


                        objTransaction.TransactionDate = date;
                        if (item.TransactionType.Contains("Debit"))
                        {
                            string Currency = item.Currency.Contains(":") ? item.Currency.Split(':')[1] : item.Currency;

                            if (item.ReceiverUserId == 0) //Account Number Case
                            {
                                string AccountNumber = EncryptDecrypt.Decrypt(item.Name);
                                objTransaction.TransactionMessage = "Send" + " " + Currency + " " + item.Amount + " " + "to" + " " + AccountNumber;
                            }
                            else
                            {

                                objTransaction.TransactionMessage = "Send" + " " + Currency + " " + item.Amount + " " + "to" + " " + item.Name;
                            }


                        }
                        else
                        {
                            string Currency = item.Currency.Contains(":") ? item.Currency.Split(':')[1] : item.Currency;

                            if (item.ReceiverUserId == 0) //Account Number Case
                            {
                                string AccountNumber = EncryptDecrypt.Decrypt(item.Name);
                                objTransaction.TransactionMessage = "Received" + " " + Currency + " " + item.Amount + " " + "from" + " " + AccountNumber;
                            }
                            else
                            {
                                objTransaction.TransactionMessage = "Received" + " " + Currency + " " + item.Amount + " " + "from" + " " + item.Name;
                            }


                        }
                        if (item.ISTXNVERIFIED)
                        {
                            objTransaction.Status = "Success";
                        }
                        else
                        {
                            objTransaction.Status = "Failed";
                        }
                        objTransaction.Amount = item.Amount;
                        lstUserTransactionHistory.Add(objTransaction);
                    }



                    #endregion

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lstUserTransactionHistory;
        }


        public ResultModel ReceiveTransactionDetails(ReceiveMoneyTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {

                #region LumentoDollar

                //var price = GetUSDPrice();

                //var getPrice = price.Result;

                //string LumenInDollarValue = getPrice.LumenInDollarAmount;

                ////we are getting balance in lumen we have to convert in usd 
                //double balance = Convert.ToDouble(objUserTransaction.Amount) * Convert.ToDouble(LumenInDollarValue);
                //String Amount = Convert.ToString(balance);
                //objUserTransaction.Amount = Amount;

                #endregion

                result.IsSuccess = true;
                result.Message = "Request Sent";


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }


        public ResultModel ReceiveTransactionDetails_V2(ReceiveMoneyTransactionModel_V2 objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {

                result.IsSuccess = true;
                result.Message = "Requested successfully";


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }

            return result;

        }

        public UserWalletResponseDetails GetUserWalletBalance(UserWalletModel wallet)
        {
            UserWalletResponseDetails userWallet = new UserWalletResponseDetails();
            try
            {

                //decrypt account no
                string AccountNumber = EncryptDecrypt.Decrypt(wallet.AccountNumber);

                var t = GetUserWalletDetails(AccountNumber);
                var task = t.Result;

                //if (task.issuccess)
                //{
                var price = GetUSDPrice();
                var BTCprice = GetCurrencyPrice("BTC");
                var getPrice = price.Result;

                string LumenInDollarValue = getPrice.LumenInDollarAmount;
                //we are getting balance in lumen we have to convert in usd 
                double balance = Convert.ToDouble(task.Balance) * Convert.ToDouble(LumenInDollarValue);
                double balanceBTC = Convert.ToDouble(task.Balance) * Convert.ToDouble(BTCprice.Result.LumenInCurrencyAmount);
                //userWallet.Balance = "$" + task.Balance;
                userWallet.Balance = "$" + Convert.ToString(balance);
                userWallet.LumenCount = task.Balance;
                userWallet.PriceInBTC = Convert.ToString(balanceBTC);
                userWallet.IsSuccess = true;
                userWallet.LumenPriceInUSD = LumenInDollarValue;
                //}
                //else
                //{
                //    userWallet.Message = task.Message;
                //}


            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return userWallet;
        }

        public async Task<LumenInDollarPrice> GetUSDPrice()
        {
            LumenInDollarPrice lumendollar = new LumenInDollarPrice();
            try
            {
                #region LumentoUSDConversion

                string apiBaseUri = ConfigurationManager.AppSettings["StellarConversionUrl"].ToString();
                var PostClient = new HttpClient();
                PostClient.BaseAddress = new Uri(apiBaseUri);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                var response = PostClient.GetAsync(apiBaseUri + "/v1/ticker/stellar/?convert=USD").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<RootResultObject>>(responseJson);

                    lumendollar.LumenInDollarAmount = result[0].price_usd;
                }


                #endregion

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lumendollar;

        }

        public async Task<LumenInCurrencyPrice> GetCurrencyPrice(string Currency)
        {
            LumenInCurrencyPrice lumenCurrency = new LumenInCurrencyPrice();
            try
            {
                #region LumentoCurrencyConversion

                string apiBaseUri = ConfigurationManager.AppSettings["StellarConversionUrl"].ToString();
                var PostClient = new HttpClient();
                PostClient.BaseAddress = new Uri(apiBaseUri);
                var response = PostClient.GetAsync(apiBaseUri + "/v1/ticker/stellar/?convert=" + Currency).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    var nameOfProperty = "price_" + Currency.ToLower();

                    var results = from x in ((Newtonsoft.Json.Linq.JArray)(result)).Children()
                                  select x[nameOfProperty];
                    lumenCurrency.LumenInCurrencyAmount = results.ToList()[0].ToString();
                }


                #endregion

            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return lumenCurrency;

        }

        public UserBeforeTransactionResponseDetails GetUserDetailsBeforeTransaction(UserDetailsBeforeTransaction usertransaction)
        {
            UserBeforeTransactionResponseDetails userTransactionDetails = new UserBeforeTransactionResponseDetails();

            try
            {
                using (var context = new WYRREntities())
                {
                    //decrypt Account Number
                    string AccountNumber = EncryptDecrypt.Decrypt(usertransaction.AccountNumber);

                    //var t = GetUserWalletDetails(usertransaction.AccountNumber);
                    var t = GetUserWalletDetails(AccountNumber);
                    var task = t.Result;

                    var price = GetUSDPrice();

                    var getPrice = price.Result;

                    string LumenInDollarValue = getPrice.LumenInDollarAmount;
                    //we are getting balance in lumen we have to convert in usd 
                    double balance = Convert.ToDouble(task.Balance) * Convert.ToDouble(LumenInDollarValue);

                    #region ServiceTaxDeduction


                    bool checkServiceTaxSetting = context.TransactionSettings.Select(x => x.IsActive).SingleOrDefault().Value;

                    double TaxAmount = 0;

                    double LumenFeesInDollar = 0;
                    string servicetax = null;
                    double Lumenfees = 0;

                    if (checkServiceTaxSetting)
                    {

                        servicetax = context.TransactionSettings.Select(x => x.ServiceTaxPerTransaction).FirstOrDefault();

                        var servicetaxwithoutpercentage = servicetax.Replace("%", "");

                        TaxAmount = Convert.ToDouble(usertransaction.Amount) * (Convert.ToDouble(servicetaxwithoutpercentage) / 100);

                    }
                    else
                    {

                        //reduce that much lumen from user balance
                        var TransactionRangeChargesList = context.TransactionRangeCharges.ToList();

                        //long? TransactionAmount = Convert.ToInt64(usertransaction.Amount);

                        ////we are getting balance in dollar we have to convert in lumen 
                        double balances = Convert.ToDouble(usertransaction.Amount) / Convert.ToDouble(LumenInDollarValue);

                        foreach (var item in TransactionRangeChargesList)
                        {

                            if (item.min < balances && item.max >= balances)
                            {
                                Lumenfees = Convert.ToDouble(item.Fee_Lumen_);
                            }

                        }
                        double FinalBalanceInLumen = Convert.ToDouble(task.Balance) - Lumenfees;
                        balance = FinalBalanceInLumen * Convert.ToDouble(LumenInDollarValue);

                        //convert lumen fee into dollar
                        LumenFeesInDollar = Lumenfees * Convert.ToDouble(LumenInDollarValue);
                    }

                    #endregion


                    if (task.IsSuccess)
                    {
                        //userTransactionDetails.TotalBalance = "$" + task.Balance;
                        userTransactionDetails.TotalBalance = "$" + Convert.ToString(balance);

                        if (checkServiceTaxSetting)
                        {
                            userTransactionDetails.isServiceTax = true;
                            userTransactionDetails.DeductionAmount = Convert.ToString(Convert.ToDouble(usertransaction.Amount) - TaxAmount);//both in dollar
                            userTransactionDetails.ServiceTax = Convert.ToString(servicetax);
                            userTransactionDetails.TaxAmount = Convert.ToString(TaxAmount);
                        }
                        else
                        {
                            userTransactionDetails.isServiceTax = false;
                            userTransactionDetails.DeductionAmount = Convert.ToString(Convert.ToDouble(usertransaction.Amount) - LumenFeesInDollar);//both in dollar
                            userTransactionDetails.LumenFee = Convert.ToString(Lumenfees);
                            userTransactionDetails.FinalLumenDeductionAmount = Convert.ToString(LumenFeesInDollar);

                        }
                        if (usertransaction.ReceiverUserID == 0)
                        {
                            userTransactionDetails.ReceiverName = usertransaction.AccountNumber;
                        }
                        else
                        {
                            userTransactionDetails.ReceiverName = context.mstUsers.Where(x => x.UserId == usertransaction.ReceiverUserID).Select(x => x.Name).SingleOrDefault();
                        }

                        userTransactionDetails.IsSuccess = true;
                    }
                    else
                    {

                        userTransactionDetails.Message = task.Message;
                    }


                }



            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
            }
            return userTransactionDetails;

        }


        #region StellarAPI

        public async Task<UserWalletResponseDetails> GetUserWalletDetails(string AccountNumber)
        {
            UserWalletResponseDetails user = new UserWalletResponseDetails();
            try
            {
                #region StellarAPI

                RootObject accountDetails = new RootObject();
                List<Balance> lstbalance = new List<Balance>();
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

                    lstbalance = result[0].Response.balances.Where(r => r.asset_type == "native").ToList();//result[0].Response.balances;

                    user.Balance = lstbalance[0].balance;
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

        public async Task<ResultModel> SendMoneyTransaction(UserTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {
                #region Decryption

                string SenderPublicKey = EncryptDecrypt.Decrypt(objUserTransaction.SenderPublicKey);
                string ReceiverPublickKey = EncryptDecrypt.Decrypt(objUserTransaction.ReceiverPublickKey);
                string SenderSecretKey = EncryptDecrypt.Decrypt(objUserTransaction.SenderSecretKey);

                #endregion

                #region StellarAPI

                RootObject accountDetails = new RootObject();

                string apiBaseUri = ConfigurationManager.AppSettings["StellarAPIBaseUrl"].ToString();
                var PostClient = new HttpClient();
                PostClient.BaseAddress = new Uri(apiBaseUri);

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("srcAcct",SenderPublicKey),
                    new KeyValuePair<string, string>("destAcct",ReceiverPublickKey),
                    new KeyValuePair<string, string>("srcSeed",SenderSecretKey),
                    new KeyValuePair<string, string>("amount",objUserTransaction.Amount)
                });

                var response = PostClient.PostAsync(apiBaseUri + "/SendPayment", formContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    List<ResultStatus> transaction = JsonConvert.DeserializeObject<MoneyObject>(responseJson).Result;

                    if (transaction[0].Status.ToLower().Trim().Contains("payment successful"))
                    {
                        result.IsSuccess = true;
                        result.Message = "Transaction done successfully";
                    }
                }
                else
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    var Result = JsonConvert.DeserializeObject<ErrorObject>(responseJson).Result;

                    result.Message = Result[0].Error;
                }


                #endregion


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;

        }

        public async Task<ResultModel> SendMoneyToMasterAccount(UserTransactionModel objUserTransaction)
        {
            ResultModel result = new ResultModel();
            try
            {
                #region Decryption

                //get StellarMasterAccount Details
                #endregion
                using (var contex = new WYRREntities())
                {
                    string SenderPublicKey = EncryptDecrypt.Decrypt(objUserTransaction.SenderPublicKey);

                    string MasterAccountPublickey = contex.StellarMasterUsers.Where(x => x.IsActive == true).Select(x => x.PublicKey).FirstOrDefault();

                    string ReceiverPublickKey = EncryptDecrypt.Decrypt(MasterAccountPublickey);
                    string SenderSecretKey = EncryptDecrypt.Decrypt(objUserTransaction.SenderSecretKey);

                    #region StellarAPI

                    RootObject accountDetails = new RootObject();

                    string apiBaseUri = ConfigurationManager.AppSettings["StellarAPIBaseUrl"].ToString();
                    var PostClient = new HttpClient();
                    PostClient.BaseAddress = new Uri(apiBaseUri);

                    //setup login data
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("srcAcct",SenderPublicKey),
                    new KeyValuePair<string, string>("destAcct",ReceiverPublickKey),
                    new KeyValuePair<string, string>("srcSeed",SenderSecretKey),
                    new KeyValuePair<string, string>("amount",Convert.ToString(objUserTransaction.TaxAmount))
                });

                    var response = PostClient.PostAsync(apiBaseUri + "/SendPayment", formContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        List<ResultStatus> transaction = JsonConvert.DeserializeObject<MoneyObject>(responseJson).Result;

                        if (transaction[0].Status.ToLower().Trim().Contains("payment successful"))
                        {
                            result.IsSuccess = true;
                            result.Message = "Transaction done successfully";
                        }
                    }
                    else
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        var Result = JsonConvert.DeserializeObject<ErrorObject>(responseJson).Result;

                        result.Message = Result[0].Error;
                    }


                    #endregion

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;

        }



        #endregion


        #region CMSMethods

        public List<CMSUserTransactionModel> GetUserTranactionDetails()
        {
            //List<CMSUserTransactionModel> lstUserTransaction = new List<CMSUserTransactionModel>();
            try
            {
                using (var context = new WYRREntities())
                {
                    var lstUserTransaction = (from userData in context.trnUserTransactionDetails.AsEnumerable()
                                              join user in context.mstUsers on userData.SenderUserId equals user.UserId
                                              where (user.IsDeleted == false)
                                              select new CMSUserTransactionModel()
                                              {
                                                  SenderUserID = userData.SenderUserId,
                                                  SenderName = context.mstUsers.Where(x => x.UserId == userData.SenderUserId).Select(x => x.Name).FirstOrDefault(),
                                                  ReceiverUserID = userData.ReceiverUserId,
                                                  ReceiverName = context.mstUsers.Where(x => x.UserId == userData.ReceiverUserId).Select(x => x.Name).FirstOrDefault(),


                                                  Amount = Convert.ToString(Math.Round(Convert.ToDouble(userData.Amount), 2)),
                                                  SenderAccountNumber = EncryptDecrypt.Decrypt(userData.SenderPublicKey),
                                                  ReceiverAccountNumber = EncryptDecrypt.Decrypt(userData.ReceiverPublicKey),
                                                  IsTxnVerified = userData.ISTXNVERIFIED,
                                                  TransactionDate = userData.TXNDATE.HasValue ? Convert.ToDateTime(userData.TXNDATE).ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty


                                              }).ToList(); ;





                    return lstUserTransaction;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        public List<TransactionSearch> GetTransactionAnalysisReport(TransactionSearch transactionSearch)
        {
            List<TransactionSearch> transactionSearchResult = new List<TransactionSearch>();
            try
            {
                using (var context = new WYRREntities())
                {
                    transactionSearchResult = (from transactionData in context.trnUserTransactionDetails.AsEnumerable()
                                               join user in context.mstUsers on transactionData.SenderUserId equals user.UserId
                                               where (transactionSearch.TransactionFromDate == null || transactionData.TXNDATE.Value.Date >= transactionSearch.TransactionFromDate.Value.Date) &&
                                               (transactionSearch.TransactionToDate == null || transactionData.TXNDATE.Value.Date <= transactionSearch.TransactionToDate.Value.Date) && (user.IsDeleted == false)

                                               select new TransactionSearch()
                                               {

                                                   SenderAccountNumber = EncryptDecrypt.Decrypt(transactionData.SenderPublicKey),
                                                   RecieverAccountNumber = EncryptDecrypt.Decrypt(transactionData.ReceiverPublicKey),
                                                   Date = transactionData.TXNDATE.HasValue ? Convert.ToDateTime(transactionData.TXNDATE).ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty,
                                                   SenderName = context.mstUsers.Where(x => x.UserId == transactionData.SenderUserId).Select(x => x.Name).FirstOrDefault(),
                                                   ReceiverName = context.mstUsers.Where(x => x.UserId == transactionData.ReceiverUserId).Select(x => x.Name).FirstOrDefault(),
                                                   TotalAmount = Convert.ToString(Math.Round(Convert.ToDouble(transactionData.Amount), 2)),
                                                   IsTxnVerified = transactionData.ISTXNVERIFIED,
                                                   ID = transactionData.ID
                                               }).ToList();



                    #region CustomerName

                    if (transactionSearch.CustomerName != string.Empty && transactionSearch.CustomerName != "" && transactionSearch.CustomerName != null)
                    {
                        transactionSearchResult = transactionSearchResult.Where(x => x.SenderName.Contains(transactionSearch.CustomerName) || (x.ReceiverName.Contains(transactionSearch.CustomerName))).ToList();
                    }

                    #endregion

                    #region Status

                    if (transactionSearch.StatusID != 0 && transactionSearch.StatusID != null)
                    {
                        if (transactionSearch.StatusID == 1)
                        {
                            transactionSearchResult = transactionSearchResult.Where(x => x.IsTxnVerified == true).ToList();
                        }
                        else
                        {
                            transactionSearchResult = transactionSearchResult.Where(x => x.IsTxnVerified == false).ToList();
                        }
                    }

                    #endregion


                }



            }
            catch (Exception ex)
            {

                throw ex;
            }
            return transactionSearchResult;
        }

        public List<StatusNameModel> GetStatusDetails()
        {
            List<StatusNameModel> lstStatus = new List<StatusNameModel>();
            try
            {
                StatusNameModel objStatus = new StatusNameModel();

                objStatus.StatusID = 1;
                objStatus.StatusName = "Success";
                lstStatus.Add(objStatus);

                StatusNameModel objFailedStatus = new StatusNameModel();

                objFailedStatus.StatusID = 2;
                objFailedStatus.StatusName = "Fail";
                lstStatus.Add(objFailedStatus);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return lstStatus;

        }

        public List<TransactionRangeCharge> GetLumenRangeDetails()
        {
            List<TransactionRangeCharge> lstRangeCharges = new List<TransactionRangeCharge>();
            try
            {
                using (var context = new WYRREntities())
                {
                    lstRangeCharges = context.TransactionRangeCharges.Where(x => x.IsDeleted == false).ToList();

                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return lstRangeCharges;

        }

        public long? ChragesFromRangeID(long ID)
        {
            long? chargefee;
            try
            {
                using (var context = new WYRREntities())
                {

                    chargefee = context.TransactionRangeCharges.Where(x => x.Id == ID).Select(x => x.Fee_Lumen_).FirstOrDefault();

                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return chargefee;
        }

        public List<TransactionSetting> GetTransactionSettingDetails()
        {
            using (var context = new WYRREntities())
            {
                var transaction = (from setting in context.TransactionSettings.ToList()
                                   select new TransactionSetting()
                                   {
                                       Id = setting.Id,
                                       TransactionPerDay = setting.TransactionPerDay,
                                       TransactionPerMonth = setting.TransactionPerMonth,
                                       ServiceTaxPerTransaction = setting.ServiceTaxPerTransaction,
                                       MinimumAmountForTransaction = setting.MinimumAmountForTransaction,
                                       PayPalMargin = setting.PayPalMargin,
                                       CoinbaseMargin = setting.CoinbaseMargin,
                                       EnableAddPayPal = setting.EnableAddPayPal,
                                       EnableWithdrawPayPal = setting.EnableWithdrawPayPal,
                                       EnableAddCoinbase = setting.EnableAddCoinbase,
                                       CoinbaseSendLimit = setting.CoinbaseSendLimit,
                                       MinimumAmountForPayPalTransaction = setting.MinimumAmountForPayPalTransaction
                                   }).ToList();
                return transaction;
            }

        }


        public TransactionSettingModel ViewTransactionSetting(long id)
        {
            TransactionSettingModel transactionSetting = new TransactionSettingModel();
            try
            {
                using (var context = new WYRREntities())
                {
                    transactionSetting = (from setting in context.TransactionSettings.AsEnumerable()
                                          where setting.Id == id
                                          select new TransactionSettingModel()
                                          {
                                              Id = setting.Id,
                                              TransactionPerDay = setting.TransactionPerDay,
                                              TransactionPerMonth = setting.TransactionPerMonth,
                                              ServiceTaxPerTransaction = setting.ServiceTaxPerTransaction.Contains("%") ? setting.ServiceTaxPerTransaction.Split('%')[0] : null,
                                              MinimumAmountForTransaction = setting.MinimumAmountForTransaction,
                                              PayPalMargin = Convert.ToInt32(setting.PayPalMargin),
                                              CoinbaseMargin = Convert.ToInt32(setting.CoinbaseMargin),
                                              EnableWithdrawPayPal = Convert.ToBoolean(setting.EnableWithdrawPayPal),
                                              EnableAddPayPal = Convert.ToBoolean(setting.EnableAddPayPal),
                                              EnableAddCoinbase = Convert.ToBoolean(setting.EnableAddCoinbase),
                                              CoinbaseSendLimit = Convert.ToInt32(setting.CoinbaseSendLimit),
                                              MinimumAmountForPayPalTransaction = setting.MinimumAmountForPayPalTransaction
                                          }).FirstOrDefault();
                    return transactionSetting;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        public int UpdateTransactionSettingDetails(TransactionSettingModel transactionsettingModel)
        {

            using (var context = new WYRREntities())
            {
                int id;

                var settingToEdit = context.TransactionSettings.Where(s => s.Id == transactionsettingModel.Id).SingleOrDefault();
                if (settingToEdit != null)
                {
                    // settingToEdit.TransactionPerDay = transactionsettingModel.TransactionPerDay;
                    //settingToEdit.TransactionPerMonth = transactionsettingModel.TransactionPerMonth;
                    settingToEdit.PayPalMargin = transactionsettingModel.PayPalMargin;
                    settingToEdit.EnableAddPayPal = transactionsettingModel.EnableAddPayPal;
                    settingToEdit.EnableWithdrawPayPal = transactionsettingModel.EnableWithdrawPayPal;
                    settingToEdit.CoinbaseMargin = transactionsettingModel.CoinbaseMargin;
                    settingToEdit.EnableAddCoinbase = transactionsettingModel.EnableAddCoinbase;
                    settingToEdit.CoinbaseSendLimit = transactionsettingModel.CoinbaseSendLimit;
                    settingToEdit.MinimumAmountForPayPalTransaction = transactionsettingModel.MinimumAmountForPayPalTransaction;
                    if (transactionsettingModel.IsChecked)
                    {
                        settingToEdit.ServiceTaxPerTransaction = transactionsettingModel.ServiceTaxPerTransaction + "%";
                        settingToEdit.MinimumAmountForTransaction = transactionsettingModel.MinimumAmountForTransaction;

                        settingToEdit.IsActive = true;
                        context.SaveChanges();
                    }
                    else
                    {
                        settingToEdit.IsActive = false;
                        context.SaveChanges();
                    }

                    if (transactionsettingModel.LumenChargeRangeID != 0 && transactionsettingModel.LumenChargeRangeID != null)
                    {
                        var transactionchargesToEdit = context.TransactionRangeCharges.Where(x => x.Id == transactionsettingModel.LumenChargeRangeID).SingleOrDefault();

                        transactionchargesToEdit.Fee_Lumen_ = transactionsettingModel.TransactionCharges;

                        context.SaveChanges();


                    }


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


        public List<UserTransactionSettingModel> GetUserTransactionSettingDetails()
        {
            using (var context = new WYRREntities())
            {
                var transaction = (from setting in context.trnUserTransactionSettings.ToList()
                                   join user in context.mstUsers on setting.UserId equals user.UserId
                                   where (user.IsDeleted == false)
                                   select new UserTransactionSettingModel()
                                   {
                                       Id = setting.Id,
                                       UserID = user.UserId,
                                       UserName = user.Name,
                                       TransactionPerDay = setting.TransactionPerDay,
                                       TransactionAmountPerDay = setting.TransactionAmountPerDay



                                   }).ToList();
                return transaction;
            }

        }


        public UserTransactionSettingModel ViewUserTransactionSetting(int? Id)
        {
            UserTransactionSettingModel transactionSetting = new UserTransactionSettingModel();
            try
            {
                using (var context = new WYRREntities())
                {
                    transactionSetting = (from setting in context.trnUserTransactionSettings
                                          join user in context.mstUsers on setting.UserId equals user.UserId
                                          where setting.Id == Id
                                          select new UserTransactionSettingModel()
                                          {
                                              Id = setting.Id,
                                              TransactionPerDay = setting.TransactionPerDay,
                                              UserName = user.Name,
                                              TransactionAmountPerDay = setting.TransactionAmountPerDay
                                          }).FirstOrDefault();
                    return transactionSetting;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public int UpdateUserTransactionSettingDetails(UserTransactionSettingModel transactionsettingModel)
        {

            using (var context = new WYRREntities())
            {
                int id;

                var settingToEdit = context.trnUserTransactionSettings.Where(s => s.Id == transactionsettingModel.Id).SingleOrDefault();
                if (settingToEdit != null)
                {
                    settingToEdit.TransactionPerDay = transactionsettingModel.TransactionPerDay;
                    settingToEdit.TransactionAmountPerDay = transactionsettingModel.TransactionAmountPerDay;

                    context.SaveChanges();

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


        #endregion



    }
}
