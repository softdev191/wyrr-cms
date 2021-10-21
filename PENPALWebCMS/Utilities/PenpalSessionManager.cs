using PENPAL.VM.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENPALWebCMS.Utilities
{
    public class PenpalSessionManager
    {
        private static UserModel _userLoginSession;
        public static UserModel UserLoginSession
        {
            get
            {
                if (HttpContext.Current.Session["UserLoginSession"] == null)
                {
                    _userLoginSession = null;
                }
                else
                {
                    _userLoginSession = HttpContext.Current.Session["UserLoginSession"] as UserModel;
                }


                return _userLoginSession;
            }
            set
            {
                HttpContext.Current.Session["UserLoginSession"] = value;
            }
        }



    }
}