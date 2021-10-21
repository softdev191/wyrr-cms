using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using PayPal.Api;

namespace PENPALWebAPI.Models
{
    public class PaypalConfig
    {

        //these variables will store the clientID and clientSecret
        //by reading them from the web.config
        public readonly static string ClientId;
        public readonly static string ClientSecret;

        static PaypalConfig()
        {
            //// var config = GetConfig();
            // ClientId = ConfigurationManager.AppSettings["clientId"];
            // ClientSecret = ConfigurationManager.AppSettings["clientSecret"];
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        //// getting properties from the web.config
        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        private static string GetAccessToken()
        {
            // getting accesstocken from paypal     
            string accessToken = new OAuthTokenCredential
          (ClientId, ClientSecret, GetConfig()).GetAccessToken();

            return accessToken;
        }

        public static APIContext GetAPIContext()
        {
            // return apicontext object by invoking it with the accesstoken
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}