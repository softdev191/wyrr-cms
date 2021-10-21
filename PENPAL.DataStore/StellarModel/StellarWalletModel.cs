using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.StellarModel
{
    public class StellarWalletModel
    {
        public class BaseAccount
        {
            public string _accountId { get; set; }
            public string sequence { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Transactions
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Operations
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Payments
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Effects
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Offers
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Trades
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Data
        {
            public string href { get; set; }
            public bool templated { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
            public Transactions transactions { get; set; }
            public Operations operations { get; set; }
            public Payments payments { get; set; }
            public Effects effects { get; set; }
            public Offers offers { get; set; }
            public Trades trades { get; set; }
            public Data data { get; set; }
        }

        public class Thresholds
        {
            public int low_threshold { get; set; }
            public int med_threshold { get; set; }
            public int high_threshold { get; set; }
        }

        public class Flags
        {
            public bool auth_required { get; set; }
            public bool auth_revocable { get; set; }
        }

        public class Balance
        {
            public string balance { get; set; }
            public string asset_type { get; set; }
        }

        public class Signer
        {
            public string public_key { get; set; }
            public int weight { get; set; }
            public string key { get; set; }
            public string type { get; set; }
        }

        public class DataAttr
        {
        }

        public class Response
        {
            public BaseAccount _baseAccount { get; set; }
            public Links _links { get; set; }
            public string id { get; set; }
            public string paging_token { get; set; }
            public string account_id { get; set; }
            public string sequence { get; set; }
            public int subentry_count { get; set; }
            public Thresholds thresholds { get; set; }
            public Flags flags { get; set; }
            public List<Balance> balances { get; set; }
            public List<Signer> signers { get; set; }
            public DataAttr data_attr { get; set; }
        }

        public class Result
        {
            public Response Response { get; set; }
        }

        public class RootObject
        {
            public List<Result> Result { get; set; }
        }




    }
}
