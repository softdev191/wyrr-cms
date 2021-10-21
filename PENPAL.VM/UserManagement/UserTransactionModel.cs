using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.VM.UserManagement
{
    public class UserTransactionModel
    {
        public int SenderUserID { get; set; }
        public string SenderName { get; set; }
        public int ReceiverUserID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string SenderPublicKey { get; set; }
        public string SenderSecretKey { get; set; }
        public string ReceiverPublickKey { get; set; }
        public double TaxAmount { get; set; }
    }

    public class UserTransactionHistoryModel
    {
        public int UserId { get; set; }
        public int PageNumber { get; set; }

    }

    public class ReceiveMoneyTransactionModel
    {
        public int SenderUserID { get; set; }
        public string SenderName { get; set; }
        public int ReceiverUserID { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ReceiverName { get; set; }
    }

    public class ReceiveMoneyTransactionModel_V2
    {
        public int SenderUserID { get; set; }
        public string SenderName { get; set; }
        public int ReceiverUserID { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ReceiverName { get; set; }
    }

    public class CMSUserTransactionModel
    {
        public long? SenderUserID { get; set; }
        public string SenderName { get; set; }
        public long? ReceiverUserID { get; set; }
        public string ReceiverName { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string SenderAccountNumber { get; set; }
        public string ReceiverAccountNumber { get; set; }
        //public DateTime? TransactionDate { get; set; }
        public string TransactionDate { get; set; }
        public bool? IsTxnVerified { get; set; }


    }


    


}
