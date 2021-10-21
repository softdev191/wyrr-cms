using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;



namespace PENPAL.DataStore.SMSHelper
{
    public class SMSManagement
    {
        public static void SendSMS(string userName, string password, string senderId, string destMobile, string message)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://www.smsjust.com/blank/sms/user/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                var result = httpClient.GetAsync(" urlsms.php?username=" + userName + "&pass=" + password + "&senderid=" + senderId + "&dest_mobileno=" + destMobile + "&message=" + message + "&response=Y").Result;
                var data = result.Content.ReadAsStringAsync().Result;
            }

           


        }

       

      



    }
}
