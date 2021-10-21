using PENPAL.DataStore.DataProviders;
using PENPAL.DataStore.Utility;
using PENPAL.VM.UserManagement;
using PENPALWebCMS.Models;
using PENPALWebCMS.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.Login
{
    [ValidateInput(false)]
    public class LoginController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: Login
        /// <summary>
        /// Get method for login page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login()
        {
            Session.Clear();
            var loginModel = new PENPAL.VM.UserManagement.LoginModel();
            return View(loginModel);
        }

        /// <summary>
        /// Post method to login a user
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginModel loginModel)
        {
            try
            {
                              
                if (ModelState.IsValid)
                {
                    _userManagementProvider = new UserManagementProvider();

                    var user = _userManagementProvider.ValidateUserName(loginModel.LoginUserName, loginModel.LoginPassword);

                    if (user != null)
                    {
                        GenerateUserSession(user);
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        Session["CurrentUser"] = null;
                        ModelState.AddModelError("", "Unable to login, please ensure that you have entered correct login details or contact helpdesk for assistance");
                        return View(loginModel);
                    }



                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            return View(loginModel);
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult LogOut()
        {

            Session.Abandon();
            Session.Clear();
            return RedirectToAction("Login", "Login");


        }

        /// <summary>
        /// Method to generate the user session
        /// </summary>
        /// <param name="userModel"></param>
        private void GenerateUserSession(UserModel userModel)
        {
            PenpalSessionManager.UserLoginSession = userModel;
            Session["CurrentUser"] = userModel;

        }




    }
}