using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.UserTransactionSetting
{
    [Permissions]
    public class UserTransactionSettingController : Controller
    {
        private UserTransactionManagementProvider _userTransactionManagementProvider = null;
        // GET: UserTransactionSetting
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetUserTransactionSettings()
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();

                var transactionSettingDetails = _userTransactionManagementProvider.GetUserTransactionSettingDetails();

                return Json(new { aaData = transactionSettingDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ViewSetting(int? Id)
        {
            try
            {
                _userTransactionManagementProvider = new UserTransactionManagementProvider();
                var transactionSettingDetails = _userTransactionManagementProvider.ViewUserTransactionSetting(Id);

                return PartialView("_ViewUserTransactionSetting", transactionSettingDetails);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }


        }

        public ActionResult ManageTransaction(int? id)
        {
            _userTransactionManagementProvider = new UserTransactionManagementProvider();
            UserTransactionSettingModel transactionsetting = null;
            try
            {

                transactionsetting = _userTransactionManagementProvider.ViewUserTransactionSetting(id);

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
            return PartialView("_ManageUserTransactionSetting", transactionsetting);

        }

        [HttpPost]
        [Permissions]
        public ActionResult ManageUserTransaction(int id, UserTransactionSettingModel transactionsettingModel)
        {
            _userTransactionManagementProvider = new UserTransactionManagementProvider();
            var success = 0;
            try
            {
                if (ModelState.IsValid)
                {
                    if (id == transactionsettingModel.Id)
                    {
                        if (transactionsettingModel.Id > 0)
                        {
                            success = _userTransactionManagementProvider.UpdateUserTransactionSettingDetails(transactionsettingModel);
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

            return PartialView("_ManageUserTransactionSetting", transactionsettingModel);
        }




    }
}