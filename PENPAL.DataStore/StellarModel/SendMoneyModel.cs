using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.StellarModel
{
    public class SendMoneyModel
    {
        public class Transaction
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Transaction transaction { get; set; }
        }

        public class Response
        {
            public Links _links { get; set; }
            public string hash { get; set; }
            public int ledger { get; set; }
            public string envelope_xdr { get; set; }
            public string result_xdr { get; set; }
            public string result_meta_xdr { get; set; }
        }

        public class ResultStatus
        {
            public string Status { get; set; }
            public Response Response { get; set; }
        }

        public class MoneyObject
        {
            public List<ResultStatus> Result { get; set; }
        }


    }
}
