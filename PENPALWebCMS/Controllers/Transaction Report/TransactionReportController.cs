using PENPAL.DataStore.CSVHelper;
using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.Transaction_Report
{
    [Permissions]
    public class TransactionReportController : Controller
    {
        private UserTransactionManagementProvider _userTransactionManagementProvider = null;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetSearchCriteria()
        {
            TransactionSearch _transaction = new TransactionSearch();
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var statusList = _userTransactionManagementProvider.GetStatusDetails();

                ViewBag.StatusList = from r in statusList
                                     select new
                                     {
                                         StatusID = r.StatusID,
                                         StatusName = r.StatusName
                                     };



            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            return PartialView("_SearchTransaction", _transaction);


        }

        public ActionResult ManageSearch(TransactionSearch _transaction)
        {

            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var transactionserachlist = _userTransactionManagementProvider.GetTransactionAnalysisReport(_transaction);


                return Json(new { aaData = transactionserachlist }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        [HttpPost]
        public bool GetSearchData(TransactionSearch _transaction)
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var transactionserachlist = _userTransactionManagementProvider.GetTransactionAnalysisReport(_transaction).Select(s => new
                {
                    SenderName = s.SenderName,
                    SenderAccountNumber = s.SenderAccountNumber,
                    ReceiverName = s.ReceiverName,
                    RecieverAccountNumber = s.RecieverAccountNumber,
                    TotalAmount = s.TotalAmount,
                    Date = s.Date,
                    IsTxnVerified = s.IsTxnVerified

                });

                var dynamicDataList = new List<object>();

                foreach (var transaction in transactionserachlist)
                {
                    var dynamicProperties = new ExpandoObject() as IDictionary<string, Object>;

                    foreach (var prop in transaction.GetType().GetProperties())
                    {
                        dynamicProperties.Add(prop.Name, prop.GetValue(transaction, null));
                    }
                    dynamicDataList.Add(dynamicProperties);
                }

                if (dynamicDataList.Count > 0)
                {
                    TempData["SearchData"] = CSVManagement.Serialize(dynamicDataList);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return false;
            }



        }

        public void DownloadSearchData()
        {
            string attachment = "attachment; filename=TransactionSummary.csv";
            HttpContext.Response.Clear();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", attachment);
            HttpContext.Response.Charset = System.Text.Encoding.UTF8.EncodingName;
            HttpContext.Response.ContentType = "text/csv";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.Unicode;
            HttpContext.Response.AddHeader("Pragma", "public");
            HttpContext.Response.Write(TempData["SearchData"].ToString());
            TempData["SearchData"] = null;
        }


    }
}