using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.APIModel
{
    /// <summary>
    /// User validation
    /// </summary>
    public class UserValidationAPIModel
    {
        public bool IsSuccess { get; set; }
        //public string Message { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailID { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Currency { get; set; }
    }

    public class UserRegistrationAPIModel
    {
        [DefaultValue(false)]
        public bool IsSuccess { get; set; }
        public int RecordID { get; set; }
        public string Message { get; set; }
        public string OTP { get; set; }
        public string ProfileImageUrl { get; set; }
        public string PublicKey { get; set; }
    }

    public class UserStellarAccountModel
    {
        public string Publickey { get; set; }
        public string SecretKey { get; set; }
    }


    public class UserContactDetails
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }
        public string Message { get; set; }
        public string UserUniqueId { get; set; }
    }

    public class CountryDetails
    {
        public long Id { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string IconImage { get; set; }
    }


    public class UserProfileDetails
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string DefaultCurrency { get; set; }
        public string Publickey { get; set; }
        public string Secretkey { get; set; }

    }

    public class UserSearchDetails
    {
        public long? UserID { get; set; }
        public long? ConnectionID { get; set; }
        public string UserName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string DefaultCurrency { get; set; }
        public string Publickey { get; set; }
        public string Secretkey { get; set; }
        public string ConnectionStatus { get; set; }
    }


    public class TransactionHistory
    {
        public string TransactionMessage { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }

    }

    public class UserWalletResponseDetails
    {
        public bool IsSuccess { get; set; }
        public string Balance { get; set; }
        public string LumenCount { get; set; }
        public string Message { get; set; }
        public string LumenPriceInUSD { get; set; }
        public string PriceInBTC { get; set; }
    }

    public class LumenInDollarPrice
    {
        public string LumenInDollarAmount { get; set; }
    }

    public class LumenInCurrencyPrice
    {
        public string LumenInCurrencyAmount { get; set; }
    }


    public class UserBeforeTransactionResponseDetails
    {
        public bool IsSuccess { get; set; }
        public string TotalBalance { get; set; }
        public string DeductionAmount { get; set; }
        public string ReceiverName { get; set; }
        public string Message { get; set; }
        public string ServiceTax { get; set; }
        public string TaxAmount { get; set; }
        public string LumenFee { get; set; }
        public string FinalLumenDeductionAmount { get; set; }
        public bool isServiceTax { get; set; }

    }


    public class ResponseModelWithID
    {
        [DefaultValue(false)]
        public bool IsSuccess { get; set; }
        //public string Message { get; set; }
        public int RecordID { get; set; }
    }
    public class ResponseModel
    {
        [DefaultValue(false)]
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }
    public class TokenResponseAPIModel
    {
        /// <summary>
        /// Access Token
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// Token Type
        /// </summary>
        public string token_type { get; set; }

        /// <summary>
        /// Expires In
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Issued
        /// </summary>
        public string issued { get; set; }

        /// <summary>
        /// Expires
        /// </summary>
        public string expires { get; set; }

        /// <summary>
        /// message
        /// </summary>
        public string message { get; set; }

    }

    public class ResponseOTPModel
    {
        [DefaultValue(false)]
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string OTP { get; set; }
    }


    public class TransactionSettings
    {
        public int EnableAddPaypal { get; set; }
        public int EnableWithdrawPaypal { get; set; }
        public int EnableAddCoinbase { get; set; }
        public string CoinbaseSendLimit { get; set; }
    }
    public class AWSCredentials
    {
        public string ClientID { get; set; }
        public string SecretKey { get; set; }
    }





}
