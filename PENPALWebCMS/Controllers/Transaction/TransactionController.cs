using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.Transaction
{
    [Permissions]
    public class TransactionController : Controller
    {
        private UserTransactionManagementProvider _userTransactionManagementProvider = null;
        // GET: Transaction
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTransactionDetails()
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var userTransactionDetails = _userTransactionManagementProvider.GetUserTranactionDetails();

               


                return Json(new { aaData = userTransactionDetails }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }


        }






    }
}