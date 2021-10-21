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

namespace PENPALWebCMS.Controllers.NewsFeed
{
    [Permissions]
    public class FeedsApprovalController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: News Feed
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

        public ActionResult GetFeeds()
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();

                var feedList = _userManagementProvider.GetNewsFeeds();

                return Json(new { aaData = feedList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ApproveFeed(long id)
        {

            try
            {
                bool status;
                _userManagementProvider = new UserManagementProvider();
                if (id > 0)
                {
                    status = _userManagementProvider.ApproveFeed(id);
                    if (status == true)
                    {
                        return Json(new { success = true, message = "Feed is approved" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, message = "Unable to approve feed" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = true, message = "Feed not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult DisApproveFeed(long id)
        {

            try
            {
                bool status;
                _userManagementProvider = new UserManagementProvider();
                if (id > 0)
                {

                    status = _userManagementProvider.DisapproveFeed(id);
                    if (status == true)
                    {
                        return Json(new { success = true, message = "Feed is disapproved" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, message = "Unable to disapprove feed" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Feed not found" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}