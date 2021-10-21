using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace PENPALWebAPI.Models
{
    public class ChangellyAPI
    {
        //private static readonly WebClient Client = new WebClient();
        private static readonly Encoding U8 = Encoding.UTF8;
        private static readonly string apiKey = ConfigurationManager.AppSettings["ChangellyClient"].ToString();//"0ab10a6233c441788ec683b1ed5327c7";
        private static readonly string apiSecret = ConfigurationManager.AppSettings["ChangellySecret"].ToString();//"d315168d1712f78cc9eebdf8cf673ae012916dac9188e18fbf4cae2792ff1819";
        private static readonly string apiUrl = "https://api.changelly.com";


        public static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public JObject GetChangellyAPIDetails(string ApiName,JObject param)
        {
            WebClient Client = new WebClient();
            JObject reqChangelly = new JObject();
            reqChangelly.Add("jsonrpc", "2.0");
            reqChangelly.Add("id", 1);
            reqChangelly.Add("method", ApiName);
            reqChangelly.Add("params", param);
 
            HMACSHA512 hmac = new HMACSHA512(U8.GetBytes(apiSecret));
            byte[] hashmessage = hmac.ComputeHash(U8.GetBytes(reqChangelly.ToString()));
            string sign = ToHexString(hashmessage);

            Client.Headers.Set("Content-Type", "application/json");
            Client.Headers.Add("api-key", apiKey);
            Client.Headers.Add("sign", sign);

            string result = Client.UploadString(apiUrl, reqChangelly.ToString());
            
            return JObject.Parse(result);
        }
    }
}