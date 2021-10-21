using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENPALWebAPI.Models
{
    public class FiatCryptoModel
    {
        public class FiatXLMReq
        {
            public decimal XLMCount { get; set; }      
        }

        public class FiatXLMRes
        {
            public decimal XLMDollarRate { get; set; }
            public decimal XLMDollarValue { get; set; }
        }

        public class ChangellyCurrency
        {
            public string currency_name { get; set; }
            public string currency_fullname { get; set; }
        }

        public class ChangellyConvReq
        {
            public string currency_name { get; set; }
            public decimal amount { get; set; }
        }
        public class ChangellyConvRes
        {
            public string amount { get; set; }
        }
        public class ChangellyCreateTxnReq
        {
            public string currency_name { get; set; }
            public decimal amount { get; set; }
            public string walletAddress { get; set; }
            public long userID { get; set; }
        }

        public class PayPalCreateTxnReq
        {
            public long userID { get; set; }
            public decimal amount { get; set; }
            public string paypalTxnID { get; set; }
            public string paypalResponse { get; set; }
            public DateTime paymentTimestamp { get; set; }
            public decimal currentXLMRate { get; set; }
        }

        public class PayPalCreateTxnRes
        {
            public string response { get; set; }

        }

        public class PayPalMinimumAmount
        {
            public string MiniumPayPalAmountinDollar { get; set; }
        }

        public class ChangellyCreateTxnRes
        {
            public string depositAddress { get; set; }
        }

        public class ChangellyCreateTxnRes_V2
        {
            public string depositAddress { get; set; }
            public string Memo { get; set; }
        }
        public class CoinbasetokenReq
        {
            public string Amt { get; set; }
            public string Currency { get; set; }
            public string userToken { get; set; }
            public long userID { get; set; }
        }

        public class CoinbasePymtReq
        {
            public string idem { get; set; }
            public string amt { get; set; }
            public string Currency { get; set; }
            public string userToken { get; set; }
            public string token_2fa { get; set; }
            public long userID { get; set; }
        }

        public class CoinbaseRes
        {
            public JObject response { get; set; }
        }

        public class CoinbaseCurReq
        {
            public decimal XLMCount { get; set; }
            public string Currency { get; set; }
        }

        public class CoinbaseCurrencies
        {
            public List<JObject> currency { get; set; }

        }
    }
}