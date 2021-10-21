using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static PENPAL.DataStore.Utility.Common;

namespace PENPAL.DataStore.Utility
{
    public class SendEmail
    {
        /// <summary>
        /// Send email by email types
        /// </summary>
        /// <param name="emailType">Email type</param>
        /// <param name="objUserDetails">User details</param>
        /// <returns>Boolean value defines mail sent or not</returns>
        //public bool SendEmailToUser(EmailType emailType, mstUser objUserDetails)
        public bool SendEmailToUser(EmailType emailType,mstUser objUserDetails, string TrnAmount)
        {
            try
            {
                SmtpDetails objSmtpDetails = new SmtpDetails();
                objSmtpDetails = GetSmtpDetails();

                MailMessage mail = new MailMessage();
                mail.To.Add(objUserDetails.UserEmail);
                mail.From = new MailAddress(objSmtpDetails.EmailID, "PENPAL Admin");
                mail.Subject = GetMailSubject(emailType);
                mail.Body = GenerateMessageBody(emailType, objUserDetails, string.Empty, TrnAmount);
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(objSmtpDetails.OutgoingMailServer);
                smtp.Port = objSmtpDetails.OutgoingServerPort; //smtp port
                smtp.Credentials = new System.Net.NetworkCredential(objSmtpDetails.EmailID, objSmtpDetails.Password); //set network username and password
                smtp.EnableSsl = objSmtpDetails.IsSSL; //use ssl

                //SmtpClient smtp = new SmtpClient();
                //smtp.Host = "smtp.gmail.com";
                //smtp.Port = 587;
                //smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

                smtp.Send(mail);    //send mail

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, Logger.LogType.Error);
                return false;
            }
        }


        /// <summary>
        /// Function select message body content
        /// </summary>
        /// <param name="emailType">EmailType</param>
        /// <param name="objUserDetails">User details</param>
        /// <returns>string</returns>
        private string GenerateMessageBody(EmailType emailType,mstUser objUserDetails, string Comments, string TrnAmount)
        {
            string strMessage = string.Empty;

            switch (emailType)
            {
                case EmailType.ForgotPassword:
                    strMessage = ReadEmailMessageFormat(objUserDetails, emailType, Comments, TrnAmount);
                    break;

                case EmailType.ChangePassword:
                    strMessage = ReadEmailMessageFormat(objUserDetails, emailType, Comments, TrnAmount);
                    break;

                case EmailType.RegisterUser:
                    strMessage = ReadEmailMessageFormat(objUserDetails, emailType, Comments, TrnAmount);
                    break;

                case EmailType.PaypalPaymentSuccess:
                    strMessage = ReadEmailMessageFormat(objUserDetails, emailType, Comments, TrnAmount);
                    break;

                case EmailType.ChangellyTickets:
                    strMessage = ReadEmailMessageFormat(objUserDetails, emailType, Comments, TrnAmount);
                    break;

                case EmailType.Exception:
                    //strMessage = ReadExceptionMessageFormat();
                    break;


                default:
                    strMessage = ReadDefaultMailMessageFormat();
                    break;
            }
            return strMessage;
        }

        /// <summary>
        /// Function to get SMTP details from Database
        /// </summary>
        /// <returns>SmtpDetails</returns>
        private SmtpDetails GetSmtpDetails()
        {
            SmtpDetails objSmtpDetails = new SmtpDetails();
            objSmtpDetails.AdminEmailId = Convert.ToString(ConfigurationManager.AppSettings["AdminEmail"]);

            objSmtpDetails.SmtpID = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpID"]);
            objSmtpDetails.OutgoingMailServer = Convert.ToString(ConfigurationManager.AppSettings["OutgoingMailServer"]);
            objSmtpDetails.LoginName = Convert.ToString(ConfigurationManager.AppSettings["LoginName"]);
            objSmtpDetails.EmailID = Convert.ToString(ConfigurationManager.AppSettings["EmailID"]);
            objSmtpDetails.Password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            objSmtpDetails.OutgoingServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["OutgoingServerPort"]);
            objSmtpDetails.IsSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]);

            return objSmtpDetails;
        }

        /// <summary>
        /// Function returns generic email format
        /// </summary>
        /// <returns>String</returns>
        private string ReadEmailMessageFormat(mstUser objUserDetails, EmailType emailType, string Comments, string TrnAmount)
        {
            string strMessage = string.Empty;
            string filePath = string.Empty;
            string EncryptedUserID = string.Empty;
            switch (emailType)
            {
                case EmailType.ForgotPassword:
                    string CMSURL = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["CMSUrl"]);
                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate/ForgotPassword.html");
                    strMessage = System.IO.File.ReadAllText(filePath);
                    strMessage = strMessage.Replace("@@cmsPath", CMSURL + "Login.aspx");
                    EncryptedUserID = !string.IsNullOrEmpty(objUserDetails.UserId.ToString()) ? EncryptDecrypt.Encrypt(objUserDetails.UserId.ToString()) : objUserDetails.UserId.ToString();
                    strMessage = string.Format(strMessage, objUserDetails.Name, objUserDetails.UserEmail, objUserDetails.Password, EncryptedUserID);
                    break;

                case EmailType.ChangePassword:
                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate/ChangePassword.html");
                    strMessage = System.IO.File.ReadAllText(filePath);
                    strMessage = strMessage.Replace("@@MobileNumber", objUserDetails.PhoneNumber.ToString());
                    strMessage = strMessage.Replace("@@Password", objUserDetails.Password.ToString());
                    EncryptedUserID = !string.IsNullOrEmpty(objUserDetails.UserId.ToString()) ? EncryptDecrypt.Encrypt(objUserDetails.UserId.ToString()) : objUserDetails.UserId.ToString();
                    strMessage = string.Format(strMessage, objUserDetails.Name, objUserDetails.UserEmail, objUserDetails.Password, EncryptedUserID);
                    break;

                case EmailType.RegisterUser:
                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate/RegisterUser.html");
                    strMessage = System.IO.File.ReadAllText(filePath);
                   // strMessage = strMessage.Replace("@@OTP", TrnAmount); //Added by NL on 18-01-2019 to send OTP in registration Email
                    EncryptedUserID = !string.IsNullOrEmpty(objUserDetails.UserId.ToString()) ? EncryptDecrypt.Encrypt(objUserDetails.UserId.ToString()) : objUserDetails.UserId.ToString();
                    strMessage = string.Format(strMessage, objUserDetails.Name, TrnAmount, objUserDetails.UserEmail, objUserDetails.Password, EncryptedUserID);//Added by NL on 18-01-2019 to send OTP in registration Email
                    break;

                case EmailType.PaypalPaymentSuccess:
                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate/PaypalPaymentSuccess.html");
                    strMessage = System.IO.File.ReadAllText(filePath);
                    //strMessage = strMessage.Replace("@@Amount", TrnAmount);
                    EncryptedUserID = !string.IsNullOrEmpty(objUserDetails.UserId.ToString()) ? EncryptDecrypt.Encrypt(objUserDetails.UserId.ToString()) : objUserDetails.UserId.ToString();
                    strMessage = string.Format(strMessage, objUserDetails.Name, TrnAmount, objUserDetails.UserEmail, objUserDetails.Password, EncryptedUserID);
                    break;

                case EmailType.ChangellyTickets:
                    filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate/ChangellyTickets.html");
                    strMessage = System.IO.File.ReadAllText(filePath);
                    strMessage = strMessage.Replace("@@Amount", TrnAmount);
                    EncryptedUserID = !string.IsNullOrEmpty(objUserDetails.UserId.ToString()) ? EncryptDecrypt.Encrypt(objUserDetails.UserId.ToString()) : objUserDetails.UserId.ToString();
                    strMessage = string.Format(strMessage, objUserDetails.Name, TrnAmount, objUserDetails.UserEmail, objUserDetails.Password, EncryptedUserID);
                    break;
                case EmailType.Exception:
                    //strMessage = ReadExceptionMessageFormat();
                    break;
                default:
                    strMessage = ReadDefaultMailMessageFormat();
                    break;
            }
            return strMessage;
        }

        /// <summary>
        /// Function to select mail subject by email type
        /// </summary>
        /// <param name="emailType">Emailtype</param>
        /// <returns>String</returns>
        private string GetMailSubject(EmailType emailType)
        {
            string strMessage = string.Empty;

            switch (emailType)
            {
                case EmailType.ForgotPassword:
                    strMessage = UserMessages.SubjectForgotPasswordMessage;
                    break;

                case EmailType.ChangePassword:
                    strMessage = UserMessages.SubjectChangePasswordMessage;
                    break;

                case EmailType.RegisterUser:
                    strMessage = UserMessages.SubjectRegistrationMessage;
                    break;

                case EmailType.PaypalPaymentSuccess:
                    strMessage = UserMessages.SubjectPaypalPaymentConfirmationMessage;
                    break;

                case EmailType.ChangellyTickets:
                    strMessage = UserMessages.SubjectChangellyTicketsConfirmationMessage;
                    break;

                case EmailType.Exception:
                    strMessage = UserMessages.SubjectExceptionMessage;
                    break;

                default:
                    strMessage = UserMessages.SubjectVerificationMessage;
                    break;
            }
            return strMessage;
        }

        /// <summary>
        /// Function returns default format for mail
        /// </summary>
        /// <returns>String</returns>
        private string ReadDefaultMailMessageFormat()
        {
            string CMSURL = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["CMSUrl"]);
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplates/Verification.html");
            string strMessage = string.Empty;
            strMessage = System.IO.File.ReadAllText(filePath);

            //         strMessage = strMessage.Replace("@@imagePath", CMSURL + "Images/SanskarLogo.png");
            strMessage = strMessage.Replace("@@cmsPath", CMSURL + "Login.aspx");

            strMessage = string.Format(strMessage, "", "", "");

            return strMessage;
        }


    }
}
