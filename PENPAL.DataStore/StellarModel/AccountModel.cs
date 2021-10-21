using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.StellarModel
{
    public class AccountModel
    {

        public class Result
        {
            public string publicKey { get; set; }
            public string secretKey { get; set; }
        }

        public class RootObject
        {
            public List<Result> Result { get; set; }
        }

    }
}
