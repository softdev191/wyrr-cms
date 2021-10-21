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

namespace PENPALWebCMS.Controllers.Administration
{
    [Permissions]
    [ValidateInput(false)]
    public class AdminController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: Admin
        public ActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult GetUsers()
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();

                var userDetails = _userManagementProvider.GetUserDetails();

                return Json(new { aaData = userDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ViewUser(int id)
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();
                var userDetails = _userManagementProvider.GetUserDetails(id);

                return PartialView("_ViewUser", userDetails);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        public ActionResult ManageUser(int id)
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();
                UserCMSModel userDetails = null;
                userDetails = _userManagementProvider.GetUserDetails(id);

                if (userDetails != null)
                {
                    ViewBag.IsUpdate = true;
                }
                else
                {
                    ViewBag.IsUpdate = false;
                }

                return PartialView("_ManageUser", userDetails);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }


        }


        [HttpPost]
        [Permissions]
        public ActionResult ManageUser(int id, UserCMSModel userModel)
        {
            _userManagementProvider = new UserManagementProvider();
            var success = 0;

            bool flag = false;
            try
            {
                ModelState.Remove("Password");
                if (ModelState.IsValid)
                {
                    if (id == userModel.UserID)
                    {

                        if (userModel.UserID > 0)
                        {

                            #region Validation

                            if (!_userManagementProvider.IsPhoneNumberExists(userModel.UserID, userModel.PhoneNumber))
                            {
                                flag = false;
                                ModelState.AddModelError("PhoneNumber", "Phone Number already exist");
                            }
                            else if (!_userManagementProvider.IsEmailAddressExists(userModel.UserID, userModel.EmailAddress))
                            {
                                flag = false;
                                ModelState.AddModelError("EmailAddress", "Email Address already exist");
                            }
                            else if (!_userManagementProvider.IsUserNameExists(userModel.UserID, userModel.UserName))
                            {
                                flag = false;
                                ModelState.AddModelError("UserName", "User name already exist");

                            }
                            else
                            {
                                flag = true;

                            }


                            #endregion

                            if (flag)
                            {

                                success = _userManagementProvider.UpdateUserRecordDetails(userModel);

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
            }
            catch (Exception ex)
            {

                throw ex;
            }

            ViewBag.IsUpdate = true;
            return PartialView("_ManageUser", userModel);
        }

        [HttpPost]
        [Permissions]
        public ActionResult Delete(long id)
        {
            try
            {
                bool status;
                _userManagementProvider = new UserManagementProvider();
                if (id > 0)
                {
                    status = _userManagementProvider.DeleteUserDetails(id);
                    if (status == true)
                    {
                        return Json(new { success = true, message = "Deleted successfully !!!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, message = "Not Deleted successfully !!!" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Invalid ID !!!" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred while deleting record !!!" }, JsonRequestBehavior.AllowGet);
            }



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
                        return Json(new { success = true, message = "User activated successfully" }, JsonRequestBehavior.AllowGet);
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


    }
}