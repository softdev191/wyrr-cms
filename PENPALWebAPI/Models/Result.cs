using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace PENPALWebAPI.Models
{
    public class Result
    {
        public HttpStatusCode status { get; set; }
        public string message { get; set; }
        public object data { get; set; }

    }

    public class ItemDetailsModel
    {
        public long ItemID { get; set; }
        public string ItemName { get; set; }
    }

   



}