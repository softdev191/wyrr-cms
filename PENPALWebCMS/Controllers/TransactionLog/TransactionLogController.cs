using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.TransactionLog
{
    [Permissions]
    public class TransactionLogController : Controller
    {
        private LogTransactionProvider _logTransactionProvider = null;
        // GET: Transaction
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTransactionLogDetails()
        {
            try
            {
                _logTransactionProvider = new LogTransactionProvider();

                var logTransactionDetails = _logTransactionProvider.GetTransactionLogDetails();

                return Json(new { aaData = logTransactionDetails }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }


        }






    }
}