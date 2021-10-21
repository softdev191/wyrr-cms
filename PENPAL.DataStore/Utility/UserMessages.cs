using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.Utility
{
    public static class UserMessages
    {
        //Email subject messages
        public static string SubjectForgotPasswordMessage = "PENPAL - Password Recovery";
        public static string SubjectChangePasswordMessage = "PENPAL - Change Password";
        public static string SubjectExceptionMessage = "PENPAL - Exception occured";
        public static string SubjectVerificationMessage = "PENPAL - Verification Email";
        public static string SubjectRegistrationMessage = "PENPAL - Registration Email";
        public static string SubjectPaypalPaymentConfirmationMessage = "PENPAL - Paypal Payment Confirmation Email";
        public static string SubjectChangellyTicketsConfirmationMessage = "PENPAL - Changelly Tickets Confirmation Email";



        public static string MailSentMessage = "{0} Email sent successfully!";
        public static string MailSentFailedMessage = "{0} mail sending failed.";

    }
}
