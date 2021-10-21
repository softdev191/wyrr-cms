using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PENPAL.DataStore.Utility
{
    public class Common
    {
        /// <summary>
        /// Model class for SMTP mail details
        /// </summary>
        public class SmtpDetails
        {
            public int SmtpID { get; set; }
            public string IncomingMailServer { get; set; }
            public string OutgoingMailServer { get; set; }
            public string LoginName { get; set; }
            public string Password { get; set; }
            public string EmailID { get; set; }
            public int OutgoingServerPort { get; set; }
            public bool IsSSL { get; set; }
            public string AdminEmailId { get; set; }
        }

        /// <summary>
        /// Enum for email types
        /// </summary>
        public enum EmailType
        {
            /// <summary>
            /// Fortgot password
            /// </summary>
            ForgotPassword = 0,

            /// <summary>
            /// Change password
            /// </summary>
            ChangePassword = 1,

            /// <summary>
            /// Exception occured
            /// </summary>
            Exception = 2,

            /// <summary>
            /// User feedback
            /// </summary>
            Feedback = 3,

            /// <summary>
            ///Register User
            /// </summary>
            RegisterUser = 4,

            /// <summary>
            ///Paypal Payment Success
            /// </summary>
            PaypalPaymentSuccess = 5,

            /// <summary>
            ///Changelly Tickets
            /// </summary>
            ChangellyTickets = 6
        }


        #region Pagination Class

        /// <summary>
        /// Pagination Class
        /// </summary>
        public static class Pagination<T> where T : class
        {
            /// <summary>
            /// Get filtered result for paging.
            /// </summary>
            /// <param name="lstItems"></param>
            /// <param name="pageNumber"></param>
            /// <param name="isNextPagePresent"></param>
            /// <returns></returns>
            public static List<T> GetFilterResult(List<T> lstItems, int pageNumber, out bool isNextPagePresent)
            {
                try
                {
                    int iPageNumber = pageNumber;
                    int iPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
                    int iNextPage = iPageNumber + 1;

                    //set value if next page present
                    isNextPagePresent = lstItems.Skip(iNextPage * iPageSize).Take(iPageSize).ToList().Count() > 0 ? true : false;

                    //filter to skip and take records by page number.
                    lstItems = lstItems.Skip(iPageNumber * iPageSize).Take(iPageSize).ToList();

                    return lstItems;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Get next page link.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lastSyncDateTime"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public static string GetNextPageLink(HttpRequestMessage request, int pageNumber)
        {
            string strNextPageLink = string.Empty;

            try
            {
                if (request != null)
                {

                    if (!string.IsNullOrEmpty(request.RequestUri.Query))
                    {
                        //form URL with Query string parameter
                        // strNextPageLink = HttpUtility.UrlDecode(request.RequestUri.OriginalString).Replace(HttpUtility.UrlDecode(request.RequestUri.LocalPath), "").Replace(HttpUtility.UrlDecode(request.RequestUri.Query), string.Empty);
                        //strNextPageLink = strNextPageLink + string.Format("{0}?pageNumber={1}", Common.GetApplicationURL(request), pageNumber + 1);
                        strNextPageLink = strNextPageLink + string.Format("pageNumber={1}", Common.GetApplicationURL(request), pageNumber + 1);
                    }
                    else
                    {
                        //form REST type URL
                        strNextPageLink = HttpUtility.UrlDecode(request.RequestUri.OriginalString).Replace(HttpUtility.UrlDecode(request.RequestUri.LocalPath), "");
                        strNextPageLink = strNextPageLink + string.Format("{0}/{1}", GetApplicationURL(request), pageNumber + 1);
                    }

                    /*
                    System.Web.HttpContextWrapper obj = ((System.Web.HttpContextWrapper)(request.Properties["MS_HttpContext"]));
                    string s = obj.Request.RawUrl;
                    string strApplicationPath = obj.Request.ApplicationPath;

                    string ss = s.Substring(0, s.IndexOf("/", 1));

                    //strApplicationPath = strApplicationPath.Length > 1 ? strApplicationPath : "" + ss;

                    if (strApplicationPath.Length > 1)
                        strApplicationPath = strApplicationPath + ss;
                    else
                        strApplicationPath = ss;
                    */

                }
            }
            catch (Exception)
            {
                throw;
            }
            return strNextPageLink;
        }

        /// <summary>
        /// Get Application URL
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationURL(HttpRequestMessage request)
        {
            string strApplicationPath = string.Empty;

            try
            {
                if (request != null)
                {
                    System.Web.HttpContextWrapper objWrapper = ((System.Web.HttpContextWrapper)(request.Properties["MS_HttpContext"]));

                    strApplicationPath = HttpUtility.UrlDecode(objWrapper.Request.ApplicationPath);
                    string strRawURL = string.Empty;

                    if (string.IsNullOrEmpty(request.RequestUri.Query))
                        strRawURL = HttpUtility.UrlDecode(objWrapper.Request.RawUrl);
                    else
                        //this is for query string style request
                        strRawURL = HttpUtility.UrlDecode(objWrapper.Request.RawUrl).Replace(HttpUtility.UrlDecode(request.RequestUri.Query), "");



                    //check local or global
                    if (strApplicationPath.Length > 1)
                        strRawURL = strRawURL.Replace(strApplicationPath, "");

                    //Get link
                    if (string.IsNullOrEmpty(request.RequestUri.Query))
                        strRawURL = strRawURL.Substring(0, strRawURL.IndexOf("/", 1));

                    if (strApplicationPath.Length > 1)
                        strApplicationPath = strApplicationPath + strRawURL;
                    else
                        strApplicationPath = strRawURL;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return strApplicationPath;
        }


        #endregion





    }
}
