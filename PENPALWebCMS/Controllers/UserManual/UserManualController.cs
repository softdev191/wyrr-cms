using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace PENPALWebCMS.Controllers.UserManual
{
    public class UserManualController : Controller
    {
        /// <summary>
        /// Download role wise manual for logged in user
        /// </summary>
        /// <returns>Download file</returns>
        public FileResult DownloadUserManual()
        {
            try
            {
                string userManualFile = string.Empty;

                userManualFile = Server.MapPath(@"~\UserManuals\" + WebConfigurationManager.AppSettings["AdminUserManual"]);

                if (System.IO.File.Exists(userManualFile))
                    return File(userManualFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(userManualFile));
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
    }
}