using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.Transaction_Setting
{
    [Permissions]
    public class TransactionConfigurationController : Controller
    {
        private UserTransactionManagementProvider _userTransactionManagementProvider = null;
        // GET: TransactionConfiguration
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetTransactionSettings()
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var transactionSettingDetails = _userTransactionManagementProvider.GetTransactionSettingDetails();

                var lumenchargeRangeList = _userTransactionManagementProvider.GetLumenRangeDetails();

                ViewBag.LumenChargeRangeList = from r in lumenchargeRangeList
                                               select new
                                               {
                                                   Id = r.Id,
                                                   Range = r.Range
                                               };



                return Json(new { aaData = transactionSettingDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }


        public ActionResult ViewSetting(int id)
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();
                var transactionSettingDetails = _userTransactionManagementProvider.ViewTransactionSetting(id);

                return PartialView("_ViewTransactionSetting", transactionSettingDetails);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }


        public ActionResult ManageTransaction(int id)
        {
            _userTransactionManagementProvider = new UserTransactionManagementProvider();
            TransactionSettingModel transactionsetting = null;
            try
            {

                transactionsetting = _userTransactionManagementProvider.ViewTransactionSetting(id);

                var lumenchargeRangeList = _userTransactionManagementProvider.GetLumenRangeDetails();

                ViewBag.LumenChargeRangeList = from r in lumenchargeRangeList
                                               select new
                                               {
                                                   Id = r.Id,
                                                   Range = r.Range
                                               };



                if (transactionsetting != null)
                {
                    ViewBag.IsUpdate = true;
                }
                else
                {
                    ViewBag.IsUpdate = false;
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            return PartialView("_ManageTransactionSetting", transactionsetting);

        }

        [HttpPost]
        [Permissions]
        public ActionResult ManageTransaction(int id, TransactionSettingModel transactionsettingModel)
        {
            _userTransactionManagementProvider = new UserTransactionManagementProvider();
            var success = 0;
            try
            {
                if (id == transactionsettingModel.Id)
                {
                    if (transactionsettingModel.Id > 0)
                    {
                        if (ModelState.IsValid)
                        {
                            success = _userTransactionManagementProvider.UpdateTransactionSettingDetails(transactionsettingModel);
                            if (success == 1)
                            {
                                return Json(new { success = true, message = "Updated successfully" });
                            }
                            else
                            {
                                return Json(new { success = true, message = "Unable to Update" });
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            var lumenchargeRangeList = _userTransactionManagementProvider.GetLumenRangeDetails();

            ViewBag.LumenChargeRangeList = from r in lumenchargeRangeList
                                           select new
                                           {
                                               Id = r.Id,
                                               Range = r.Range
                                           };

            return PartialView("_ManageTransactionSetting", transactionsettingModel);
        }


        public JsonResult GetChargesFromRange(long rangeId)
        {
            _userTransactionManagementProvider = new UserTransactionManagementProvider();
            try
            {

                long? lumenchargeRangeList = _userTransactionManagementProvider.ChragesFromRangeID(rangeId);

                return Json(new { success = true, fee=lumenchargeRangeList});

            }
            catch (Exception ex)
            {

                throw ex;
            }
           

        }


        //public ActionResult GetSearchCriteria()
        //{
        //    TransactionSetting _transaction = new TransactionSetting();
        //    try
        //    {
        //        _userTransactionManagementProvider = new UserTransactionManagementProvider();





        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //    return PartialView("_TransactionConfiguration", _transaction);


        //}

    }
}