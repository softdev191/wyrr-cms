using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace PENPAL.DataStore.Templates
{
    public class EmailTemplates
    {
        public static string UserRegistrationEmail()
        {
            StringBuilder oStringBuilder = new StringBuilder();
            oStringBuilder.Append("Dear User,");
            oStringBuilder.Append("<br/><br/>");
            oStringBuilder.Append("Welcome!<br/><br/>You have registered at WYRR");
            oStringBuilder.Append(". Please find below login details:");
            oStringBuilder.Append("<br/><br/>");
            oStringBuilder.Append("Username: ");
            oStringBuilder.Append("&nbsp<b><I>");
            oStringBuilder.Append("{0}");
            oStringBuilder.Append("</b></I><br/><br/>");
            oStringBuilder.Append("Password: ");
            oStringBuilder.Append("&nbsp<b><I>");
            oStringBuilder.Append("{1}");
            oStringBuilder.Append("</b></I><br/><br/><br/>");
            oStringBuilder.Append("Regards,");
            oStringBuilder.Append("<br/>");
            oStringBuilder.Append("WYRR Team");
            return oStringBuilder.ToString();
        }

        
    }
}
