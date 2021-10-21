using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class TransactionLogModel
    {
        public string TxnID { get; set; }
        public string TxnType { get; set; }
        public string UserName { get; set; }     
        public string TxnDate { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal AmountGiven { get; set; }
        public string CurrencyGiven { get; set; }
        public decimal AmountReceived { get; set; }
        public string CurrencyReceived { get; set; }
        public string PaypalClientMargin { get; set; }
        public string PaypalMargin { get; set; }
        public decimal? PaypalClientMarginAmt { get; set; }
        public decimal? PaypalMarginAmt { get; set; }
        public string ChangellyClientMargin { get; set; }
        public string ChangellyMargin { get; set; }
        public decimal? ChangellyClientMarginAmt { get; set; }
        public decimal? ChangellyMarginAmt { get; set; }
        public string TxnStatus { get; set; }
    }
    
}
