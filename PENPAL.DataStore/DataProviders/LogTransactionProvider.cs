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
using PENPAL.DataStore.EmailHelper;

namespace PENPAL.DataStore.DataProviders
{
    public class LogTransactionProvider
    {
        public static WYRREntities db = new WYRREntities();
        public ResponseModel InsertLogTransaction(logTransactionDetail logDetail)
        {
            ResponseModel result = new ResponseModel();

            try
            {
                 logTransactionDetail objlogTransaction = new logTransactionDetail();

                using (var context = new WYRREntities())
                {
                    var checkTransaction = context.logTransactionDetails.Where(x => x.TxnID == logDetail.TxnID).FirstOrDefault();

                    if (checkTransaction == null)
                    {
                        #region NewLog
                        
                        context.logTransactionDetails.Add(logDetail);
                        context.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = "Log Added Successfully";
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

        public ResponseModel UpdateLogTransaction(logTransactionDetail logDetail)
        {
            ResponseModel result = new ResponseModel();

            try
            {
                logTransactionDetail objlogTransaction = new logTransactionDetail();

                using (var context = new WYRREntities())
                {
                    var checkTransaction = context.logTransactionDetails.Where(x => x.TxnID == logDetail.TxnID).FirstOrDefault();

                    if (checkTransaction == null)
                    {
                        #region NewLog
                        checkTransaction.TxnStatus = logDetail.TxnStatus;
                       // context.logTransactionDetails.Add(logDetail);
                        context.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = "Log Updated Successfully";
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


        public List<TransactionLogModel> GetTransactionLogDetails()
        {
            //List<CMSUserTransactionModel> lstUserTransaction = new List<CMSUserTransactionModel>();
            try
            {
                using (var context = new WYRREntities())
                {
                   // var logTransactionDetails = context.logTransactionDetails.OrderByDescending(r=>r.TxnDate).ToList(); ;
                    //return logTransactionDetails;
                    var logTransactionDetails = (from logTxnData in context.logTransactionDetails.AsEnumerable()
                                              join user in context.mstUsers on logTxnData.UserID equals user.UserId
                                              select new TransactionLogModel()
                                              {
                                                  TxnID = logTxnData.TxnID,
                                                  TxnType = logTxnData.TxnType,
                                                  UserName = context.mstUsers.Where(x => x.UserId == logTxnData.UserID).Select(x => x.Name).FirstOrDefault(),
                                                  TxnDate =logTxnData.TxnDate.ToString(),
                                                  SenderAddress= logTxnData.SenderAddress,
                                                  ReceiverAddress= logTxnData.ReceiverAddress,
                                                  AmountGiven= logTxnData.AmountGiven,
                                                  CurrencyGiven= logTxnData.CurrencyGiven,
                                                  AmountReceived= logTxnData.AmountReceived,
                                                  CurrencyReceived= logTxnData.CurrencyReceived,
                                                  PaypalClientMargin= logTxnData.PaypalClientMargin,
                                                  PaypalMargin= logTxnData.PaypalMargin,
                                                  PaypalClientMarginAmt= logTxnData.PaypalClientMarginAmt,
                                                  PaypalMarginAmt= logTxnData.PaypalMarginAmt,
                                                  ChangellyClientMargin= logTxnData.ChangellyClientMargin,
                                                  ChangellyClientMarginAmt= logTxnData.ChangellyClientMarginAmt,
                                                  ChangellyMargin= logTxnData.ChangellyMargin,
                                                  ChangellyMarginAmt= logTxnData.ChangellyMarginAmt,
                                                  TxnStatus= logTxnData.TxnStatus
                                              }).ToList(); ;





                    return logTransactionDetails;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
    }
}
