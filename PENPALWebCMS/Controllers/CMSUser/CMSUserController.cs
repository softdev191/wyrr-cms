using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace PENPALWebCMS.Controllers.CMSUser
{
    public class CMSUserController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: CMSUser
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCMSUsers()
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();

                var userDetails = _userManagementProvider.GetCMSUserDetails();

                return Json(new { aaData = userDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ViewCMSUser(int id)
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();
                var userDetails = _userManagementProvider.GetCMSUserDetails(id);

                return PartialView("_ViewCMSUser", userDetails);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ManageCMSUser(int? id)
        {
            _userManagementProvider = new UserManagementProvider();

            UserCMSModel userDetails = null;
            try
            {
                if (id != null)
                {
                    //Edit Case
                    userDetails = _userManagementProvider.GetCMSUserDetails(id);
                    ViewBag.IsUpdate = true;
                }
                else
                {
                    //add user case
                    userDetails = new UserCMSModel();
                    // New request for create new User 
                    ViewBag.IsUpdate = true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return PartialView("_ManageCMSUser", userDetails);
        }

        [HttpPost]
        public ActionResult ManageCMSUser(int? id, UserCMSModel userModel)
        {
            _userManagementProvider = new UserManagementProvider();
            var success = 0;
            bool flag = false;
            string password;
            try
            {
                ModelState.Remove("Password");
                if (ModelState.IsValid)
                {

                    if (userModel.UserID > 0)
                    {
                        //Edit Case
                        #region Validation

                        if (!_userManagementProvider.IsPhoneNumberExists(userModel.UserID, userModel.PhoneNumber))
                        {
                            flag = true;
                            ModelState.AddModelError("PhoneNumber", "Phone Number already exist");
                        }
                        else if (!_userManagementProvider.IsEmailAddressExists(userModel.UserID, userModel.EmailAddress))
                        {
                            flag = true;
                            ModelState.AddModelError("EmailAddress", "Email Address already exist");
                        }
                        else if (!_userManagementProvider.IsUserNameExists(userModel.UserID, userModel.UserName))
                        {
                            flag = true;
                            ModelState.AddModelError("UserName", "User name already exist");

                        }
                        else
                        {
                            flag = false;

                        }

                        #endregion

                        if (!flag)
                        {

                            success = _userManagementProvider.UpdateCMSUserRecordDetails(userModel);

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
                    else
                    {

                        #region Validation

                        if (!_userManagementProvider.IsPhoneNumberExists(userModel.UserID, userModel.PhoneNumber))
                        {
                            flag = true;
                            ModelState.AddModelError("PhoneNumber", "Phone Number already exist");
                        }
                        else if (!_userManagementProvider.IsEmailAddressExists(userModel.UserID, userModel.EmailAddress))
                        {
                            flag = true;
                            ModelState.AddModelError("EmailAddress", "Email Address already exist");
                        }
                        else if (!_userManagementProvider.IsUserNameExists(userModel.UserID, userModel.UserName))
                        {
                            flag = true;
                            ModelState.AddModelError("UserName", "User name already exist");

                        }
                        else
                        {
                            flag = false;

                        }

                        #endregion
                        //Add case

                        if (!flag)
                        {
                            password = userModel.Password.Trim();
                            success = _userManagementProvider.AddCMSUserDetails(userModel);

                            if (success == 1)
                            {
                                return Json(new { success = true, message = "Added successfully" });
                            }
                            else
                            {
                                return Json(new { success = true, message = "Unable to Add" });
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
            return PartialView("_ManageCMSUser", userModel);
        }

        [HttpPost]
        public ActionResult Active(long id)
        {
            try
            {
                bool status;
                _userManagementProvider = new UserManagementProvider();
                if (id > 0)
                {
                    status = _userManagementProvider.ActivateUserDetails(id);
                    if (status == true)
                    {
                        return Json(new { success = true, message = "CMS user activated successfully" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, message = "Unable to activate user" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = true, message = "User Id not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult DeActive(long id)
        {

            try
            {
                bool status;
                _userManagementProvider = new UserManagementProvider();
                if (id > 0)
                {

                    status = _userManagementProvider.DeactivateUserDetails(id);
                    if (status == true)
                    {
                        return Json(new { success = true, message = "Deactivated successfully" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, message = "Unable to deactivate user" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, message = "User Id not found" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ChangePassword()
        {
            return View(new ResetPassword());
        }

        [HttpPost]
        public ActionResult ChangePassword(ResetPassword resetPasswordModel)
        {
            _userManagementProvider = new UserManagementProvider();

            bool status;
            if (ModelState.IsValid)
            {
                resetPasswordModel.UserID = PENPALWebCMS.Utilities.PenpalSessionManager.UserLoginSession.UserID;
                status = _userManagementProvider.ResetPassword(resetPasswordModel);
                if (status == true)
                {
                    return Json(new { success = true, message = "Password reset successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = "Unable to reset Password" }, JsonRequestBehavior.AllowGet);
                }
            }
            return View(resetPasswordModel);

        }


    }
}