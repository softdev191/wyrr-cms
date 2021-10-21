using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class TransactionSearch
    {

        public Nullable<System.DateTime> TransactionFromDate { get; set; }
        public Nullable<System.DateTime> TransactionToDate { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Date { get; set; }
        public string TotalAmount { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string SenderAccountNumber { get; set; }
        public string RecieverAccountNumber { get; set; }
        public bool? IsTxnVerified { get; set; }
        public long ID { get; set; }
        public long? StatusID { get; set; }
    }

    public class StatusNameModel
    {
        public long StatusID { get; set; }
        public string StatusName { get; set; }


    }

    public class TransactionSettingModel
    {
        public long Id { get; set; }
        public long? TransactionPerDay { get; set; }
        public long? TransactionPerMonth { get; set; }
        [Required(ErrorMessage = "Please enter service tax")]
        [RegularExpression(@"^[0-9%]*$", ErrorMessage = "Please enter valid service tax")]
        public string ServiceTaxPerTransaction { get; set; }
        [Required(ErrorMessage = "Please enter minimum amount for transaction")]
        [RegularExpression(@"^[0-9.]*$", ErrorMessage = "Please enter valid minimum amount for transaction")]
        public decimal? MinimumAmountForTransaction { get; set; }
        [Required(ErrorMessage = "Please enter percentage for Paypal Margin")]
        [Range(0,100,ErrorMessage ="Please enter percentage value")]
        public int PayPalMargin { get; set; }
        [Required(ErrorMessage = "Please enter percentage for Coinbase Margin")]
        [Range(0, 100, ErrorMessage = "Please enter percentage value")]
        public int CoinbaseMargin { get; set; }
        [Required(ErrorMessage = "Please enter Coinbase send limit")]
        [Range(1, 500, ErrorMessage = "Please enter value")]
        public int CoinbaseSendLimit { get; set; }
        public bool EnableAddPayPal { get; set; }
        public bool EnableWithdrawPayPal { get; set; }
        public bool EnableAddCoinbase { get; set; }
        public Nullable<long> LumenChargeRangeID { get; set; }
        public Nullable<long> TransactionCharges { get; set; }
        public bool IsChecked { get; set; }
        [RegularExpression(@"^[0-9.]*$", ErrorMessage = "Please enter valid minimum amount for paypal transaction")]
        public decimal? MinimumAmountForPayPalTransaction { get; set; }
        
    }

    public class UserTransactionSettingModel
    {
        public long Id { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter user transaction per day")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter valid transaction per day")]
        public long? TransactionPerDay { get; set; }
        [Required(ErrorMessage = "Please enter user transaction amount per day")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter valid transaction per day")]
        public decimal? TransactionAmountPerDay { get; set; }

    }


}
