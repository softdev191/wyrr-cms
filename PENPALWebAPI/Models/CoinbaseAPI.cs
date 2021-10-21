using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace PENPALWebAPI.Models
{
    public class CoinbaseAPI
    {
        //private static readonly WebClient Client = new WebClient();
        private static readonly Encoding U8 = Encoding.UTF8;
        private static readonly string apiKey = ConfigurationManager.AppSettings["CoinbaseClient"].ToString();// "obbdDXqa3arZ9VvA";
        private static readonly string apiSecret = ConfigurationManager.AppSettings["CoinbaseSecret"].ToString();//"CUj4Wh3QXTSf1G0AxiBeYdgzgp9R0hrG";
        private static readonly string apiUrl = "https://api.coinbase.com";

        public static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public JObject GetcoinbaseAPIDetails(string Method, string Requestpath, string body)
        {
            WebClient Client = new WebClient();
            JObject jo = new JObject();

            //JObject reqChangelly = new JObject();
            //reqChangelly.Add("jsonrpc", "2.0");
            //reqChangelly.Add("id", 1);
            //reqChangelly.Add("method", ApiName);
            //reqChangelly.Add("params", param);
            string result1 = Client.DownloadString(apiUrl + "/v2/time");

            var timestamp = JObject.Parse(result1)["data"]["epoch"];

            string message = timestamp + Method + Requestpath + body;
            HMACSHA256 hmac = new HMACSHA256(U8.GetBytes(apiSecret));
            byte[] hashmessage = hmac.ComputeHash(U8.GetBytes(message));
            string sign = ToHexString(hashmessage);

            Client.Headers.Add("CB-VERSION", "2018-03-20");
            Client.Headers.Add("CB-ACCESS-KEY", apiKey);
            Client.Headers.Add("CB-ACCESS-SIGN", sign);
            Client.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp.ToString());
            Client.Headers.Set("Content-Type", "application/json");
            string result = string.Empty;
            try
            {
                if (Method == "GET")
                {
                    result = Client.DownloadString(apiUrl + Requestpath);
                }
                else
                {

                    result = Client.UploadString(apiUrl + Requestpath, null);
                }
                jo["status"] = "Success";
                jo["responseBody"] = JObject.Parse(result);
                return jo;
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                dynamic obj = JsonConvert.DeserializeObject(resp);

                jo["status"] = "error";
                jo["responseBody"] = JObject.Parse(Convert.ToString(obj["errors"][0]));
                //var messageFromServer = obj.error.message;
                return jo;
            }


        }

        public string GetMerchantAddress()
        {

            var result = GetcoinbaseAPIDetails("GET", "/v2/accounts", "");  //Client.DownloadString(apiUrl + "/v2/accounts");

            var results = from x in result["responseBody"]["data"].Children()
                          where x["type"].Value<string>() == "wallet"
                          && x["currency"]["code"].Value<string>() == "BTC"
                          select x;


            var btcadd = GetcoinbaseAPIDetails("GET", "/v2/accounts/" + results.First()["id"] + "/addresses", ""); //Client.DownloadString(apiUrl + "/v2/accounts/" + results.First()["id"] + "/addresses");

            var merchantAddress = from x in btcadd["responseBody"]["data"].Children()
                                  where x["network"].Value<string>() == "bitcoin"
                                  select x;

            return merchantAddress.FirstOrDefault()["address"].ToString();
            // return JObject.Parse(merchantAddress.ToString());
        }

        public JObject GetcoinbaseUserAPIDetails(string userToken, string token_2fa, string Method, string Requestpath, JObject data)
        {
            WebClient Client = new WebClient();
            Client.Headers.Set("Authorization", "Bearer " + userToken);
            Client.Headers.Add("CB-VERSION", "2018-03-02");

            JObject jo = new JObject();

            if (token_2fa.Length > 0)
            {
                Client.Headers.Set("CB-2FA-TOKEN", token_2fa);
            }
            try
            {
                string result = string.Empty;
                if (Method == "GET")
                {
                    result = Client.DownloadString(apiUrl + Requestpath);
                }
                else
                {

                    result = Client.UploadString(apiUrl + Requestpath, data.ToString());
                }
                jo["status"] = "Success";
                jo["responseBody"] = JObject.Parse(result);
                return jo;
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                dynamic obj = JsonConvert.DeserializeObject(resp);
               
                jo["status"] = "error";
                jo["responseBody"] = JObject.Parse(Convert.ToString(obj["errors"][0]));
                //var messageFromServer = obj.error.message;
                return jo;
            }

        }
    }
}
