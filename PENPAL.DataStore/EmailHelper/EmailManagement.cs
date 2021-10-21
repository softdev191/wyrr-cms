using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.EmailHelper
{
    public class EmailManagement
    {
        public static void SendEmail_Background(string strTo, string strFrm, string strCC, string strSubject, string strBody)
        {
            try
            {
                string _strTo = string.Empty;
                string _strFrm = string.Empty;
                string _strCC = string.Empty;
                string _strSubject = string.Empty;
                string _strBody = string.Empty;
                string _FilePath = string.Empty;
                _strTo = strTo;
                _strFrm = strFrm;
                _strCC = strCC;
                _strSubject = strSubject;
                _strBody = strBody;


                string strFrom = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["eMailFrom"]);
                string SMTPServerName = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPAddress"]);
                string strSMTPUser = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPUser"]);
                string strSMTPPass = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPPass"]);


                //string strFrom = strFrm;

                System.Net.Mail.MailMessage objMM = new System.Net.Mail.MailMessage();
                Attachment FileAttachement = null;

                // configure your mail appearance 
                int NoOfRecipients;
                string[] ListOfMailIDs;
                NoOfRecipients = _strTo.Split(';').Length;
                ListOfMailIDs = _strTo.Split(';');

                for (int i = 0; i < NoOfRecipients; i++)
                {
                    objMM.To.Add(ListOfMailIDs[i].ToString());
                }
                if (!string.IsNullOrEmpty(_strCC))
                {
                    NoOfRecipients = _strCC.Split(';').Length;
                    ListOfMailIDs = _strCC.Split(';');

                    for (int i = 0; i < NoOfRecipients; i++)
                    {
                        objMM.CC.Add(ListOfMailIDs[i].ToString());
                    }
                }
                //objMM.CC.Add(_strCC);


                objMM.From = new System.Net.Mail.MailAddress(strFrom);
                objMM.IsBodyHtml = true;
                objMM.Subject = _strSubject;
                objMM.Body = _strBody;
                objMM.Priority = MailPriority.High;

                if (!string.IsNullOrEmpty(_FilePath))
                {
                    FileAttachement = new Attachment(_FilePath);
                    objMM.Attachments.Add(FileAttachement);
                }

                // define smtp and authentication credential 
                try
                {

                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();

                    smtp.Host = SMTPServerName;
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    if (!string.IsNullOrEmpty(strSMTPUser))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential(strSMTPUser, strSMTPPass);
                    }

                    // send the mail now 
                    smtp.Send(objMM);
                    //IsMailSent = true;
                    //' Clear the attchment object and file name 
                    objMM.Attachments.Clear();
                    //FileName = null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //FileAttachement = null;
                //return IsMailSent;
            }
            catch (Exception)
            {

            }
        }


    }
}
