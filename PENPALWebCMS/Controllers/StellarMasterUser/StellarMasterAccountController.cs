using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.StellarMasterUser
{
    public class StellarMasterAccountController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: StellarMasterAccount
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetMasterUsers()
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();

                var userDetails = _userManagementProvider.GetStellarMasterUserDetails();

                return Json(new { aaData = userDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ViewStellarUser(long id)
        {
            try
            {
                try
                {
                    _userManagementProvider = new UserManagementProvider();
                    var userDetails = _userManagementProvider.GetStellarMasterUserDetails(id);

                    return PartialView("_ViewStellarMasterUser", userDetails);

                }
                catch (Exception ex)
                {
                    Logger.Log(ex, Logger.LogType.Error);
                    throw ex;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }



        }

        public ActionResult ManageStellarUser(long id)
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();
                MasterUserModel userDetails = null;
                userDetails = _userManagementProvider.GetStellarMasterUserDetails(id);

                if (userDetails != null)
                {
                    ViewBag.IsUpdate = true;
                }
                else
                {
                    ViewBag.IsUpdate = false;
                }

                return PartialView("_ManageStellarMasterUser", userDetails);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }


        }

        [HttpPost]
        [Permissions]
        public ActionResult ManageStellarUser(int id, MasterUserModel userModel)
        {
            _userManagementProvider = new UserManagementProvider();
            var success = 0;


            try
            {

                if (ModelState.IsValid)
                {
                    if (id == userModel.ID)
                    {

                        if (userModel.ID > 0)
                        {

                            success = _userManagementProvider.UpdateMasterUserRecordDetails(userModel);

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

                throw ex;
            }

            ViewBag.IsUpdate = true;
            return PartialView("_ManageStellarMasterUser", userModel);
        }

    }
}