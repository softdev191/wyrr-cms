using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore
{
    public class ResultModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }

    public class ResultOTPModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string OTP { get; set; }
    }

    /// <summary>
    /// Result model with identifier.
    /// </summary>
    public class ResultModelWithID
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int RecordID { get; set; }
    }

    //public class UserRegistrationAPIModel
    //{
    //    [DefaultValue(false)]
    //    public bool IsSuccess { get; set; }
    //    public int RecordID { get; set; }
    //    public string OTP { get; set; }
    //    public string Publickey { get; set; }
    //    public string SecretKey { get; set; }

    //}


}
