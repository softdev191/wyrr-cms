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

namespace PENPALWebCMS.Controllers.User_Report
{
    [Permissions]
    public class UserReportController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetSearchCriteria()
        {
            UserAnalysisModel _user = new UserAnalysisModel();
            try
            {
                _userManagementProvider = new UserManagementProvider();

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }
            return PartialView("_SearchUser", _user);


        }

        public ActionResult ManageSearch(UserAnalysisModel _user)
        {
            try
            {

                _userManagementProvider = new UserManagementProvider();
                var userSearchList = _userManagementProvider.GetUserAnalysisDetails(_user);
                return Json(new { aaData = userSearchList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                throw ex;
            }



        }


        [HttpPost]
        public bool GetSearchData(UserAnalysisModel _user)
        {
            try
            {
                _userManagementProvider = new UserManagementProvider();

                var transactionserachlist = _userManagementProvider.GetUserAnalysisDetails(_user).Select(s => new
                {
                    UserName = s.UserName,
                    EmailAddress = s.EmailAddress,
                    AccountNumber = s.AccountNumber,
                    PhoneNumber = s.PhoneNumber,
                    UniqueUserId = s.UniqueUserId,
                    IsSocialLogin = s.IsSocialLogin,
                    DefaultCurrency = s.DefaultCurrency,
                    Date = s.Date



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
            string attachment = "attachment; filename=AppUserSummary.csv";
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