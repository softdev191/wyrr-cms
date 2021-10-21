using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PENPAL.DataStore.Utility
{
    public static class Logger
    {
        public static WYRREntities db = new WYRREntities();
        #region Global Declaration Section

        public enum LogType { Debug, Info, Warn, Error, Fatal }

        #endregion

        #region Public Methods

        /// <summary>
        /// Write a log entry in database
        /// </summary>
        /// <param name="contents">Actual contents to log</param>
        /// <param name="type">Log type</param>.
        public static void Log(object contents, LogType type)
        {
            try
            {
               // int minLogLevel = Convert.ToInt32(ConfigurationManager.AppSettings["minLogLevel"]);
              //  if ((int)type >= minLogLevel)
               // {
                    if (HttpContext.Current.Session == null)
                        writeContents(contents, type, null);
                    else
                        writeContents(contents, type, null);
                //}
            }
            catch
            {
                // Do nothing
            }
        }

        /// <summary>
        ///  This method will write the log entries for imported and processed records
        /// </summary>
        /// <param name="sPathName"></param>
        /// <param name="FileName"></param>
        /// <param name="DateTime"></param>
        /// <param name="Status"></param>
        /// <param name="objError"></param>
        /// <remarks></remarks>
        public static void ErrLogs(string sPathName, string DateTime, string Status, string objError)
        {
            try
            {
                string strLogFormat = "";
                string strErrDay = null;
                string strErrMonth = null;
                string strErrYear = null;
                string strErrDate = null;
                //This function will be called every time a Error occures in the sytem.
                //This function opens a txt file under the iis root of the applcation and logges the Error details available under 
                //Err.Message, Err.Source, Err.Number, Logged in User, Error Time.


                //This function will be called every time a Error occures in the sytem.
                //This function opens a txt file under the iis root of the applcation and logges the Error details available under
                //Err.Message, Err.Source, Err.Number, Logged in User, Error Time.
                System.IO.StreamWriter swErr = default(System.IO.StreamWriter);
                //strLogFormat = Now.Date.ToString & Now.TimeOfDay.ToString & " ===> "
                strLogFormat += "=======================================================================================================" + Environment.NewLine;
                strLogFormat += "=============                             Log Entry                    ================================" + Environment.NewLine;
                strLogFormat += "========================================================================================================" + Environment.NewLine;
                strLogFormat += " Date      |    Status    | Error Occured(If Any)   | Operation Time   " + Environment.NewLine;
                //Now.TimeOfDay.ToString & " ===> "
                strLogFormat += "========================================================================================================";
                strErrDay = System.DateTime.Now.Day.ToString();
                strErrMonth = System.DateTime.Now.Month.ToString();
                strErrYear = System.DateTime.Now.Year.ToString();
                strErrDate = strErrDay + "-" + strErrMonth + "-" + strErrYear;
                bool fileExists = false;
                fileExists = System.IO.File.Exists(sPathName + "\\" + strErrDate + ".txt");
                if ((fileExists == false))
                {
                    swErr = new System.IO.StreamWriter(sPathName + "\\" + strErrDate + ".txt", true);
                    swErr.WriteLine(strLogFormat + Environment.NewLine);
                    //)+ objError.ToString + vbCrLf)
                    swErr.WriteLine(DateTime + "|" + Status + "|" + objError + "|" + System.DateTime.Now.TimeOfDay.Hours + ":" + System.DateTime.Now.TimeOfDay.Minutes + ":" + System.DateTime.Now.TimeOfDay.Seconds);
                    swErr.WriteLine("========================================================================================================");
                }
                else
                {
                    swErr = new System.IO.StreamWriter(sPathName + "\\" + strErrDate + ".txt", true);
                    swErr.WriteLine(DateTime + "|" + Status + "|" + objError + "|" + System.DateTime.Now.TimeOfDay.Hours + ":" + System.DateTime.Now.TimeOfDay.Minutes + ":" + System.DateTime.Now.TimeOfDay.Seconds);

                    swErr.WriteLine("========================================================================================================");
                }
                //swErr.WriteLine(FileName & "|" & DateTime & "|" & Status & "|" & objError& Format(Now.Date, "dd-MM-yyyy") & " " & Now.TimeOfDay.Hours & ":" & Now.TimeOfDay.Minutes & ":" & Now.TimeOfDay.Seconds))
                swErr.Flush();
                swErr.Close();
            }
            catch
            {
            }
        }

        #endregion

        #region Private Methods
        private static string GetIP()
        {
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            IPAddress[] addr = ipEntry.AddressList;

            return addr[addr.Length - 1].ToString();

        }

        private static void writeContents(object contents, LogType type, long? userId)
        {
            string ipAddress = GetIP();

            try
            {
                using (var context=new WYRREntities())
                {
                    if (contents is Exception)
                    {
                        Exception ex = (Exception)contents;

                        ErrorLog log = new ErrorLog();
                        log.LogType = Convert.ToString(type);
                        log.IpAddress = ipAddress;
                        log.Message = Convert.ToString(ex.Message);
                        log.StackTrace = Convert.ToString(ex.StackTrace);
                        log.InnerException = Convert.ToString(ex.InnerException);
                        log.ExceptionType = Convert.ToString(ex.GetType());
                        log.CreatedDate = DateTime.UtcNow;
                        context.ErrorLogs.Add(log);
                        context.SaveChanges();


                        log = null;

                        string FileName = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/");
                        ErrLogs(FileName, System.DateTime.Now.ToShortDateString(), type.ToString(), ipAddress + " " + ex.Message + " " + ex.StackTrace + " " + ex.InnerException + " " + ex.GetType().ToString());
                    }
                    else
                    {
                        System.Diagnostics.StackTrace stack = new StackTrace();
                        int stackIdx = -1;
                        // step over all references to methods in this class
                        string myTypeName = "Logger";
                        while (stack.GetFrame(++stackIdx).GetMethod().DeclaringType.Name.Equals(myTypeName)) ;

                        string callingObjectName = stack.GetFrame(stackIdx).GetMethod().DeclaringType.Name;
                        string callingMethodName = stack.GetFrame(stackIdx).GetMethod().Name;

                        ErrorLog log = new ErrorLog();
                        log.LogType = Convert.ToString(type);
                        log.IpAddress = ipAddress;
                        log.Message = Convert.ToString(contents);
                        log.StackTrace = string.Format("{0}::{1}", callingObjectName, callingMethodName);
                        log.InnerException = "";
                        log.ExceptionType = "";
                        log.CreatedDate = DateTime.UtcNow;

                        context.ErrorLogs.Add(log);
                        context.SaveChanges();

                        log = null;

                        string FileName = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/");
                        ErrLogs(FileName, System.DateTime.Now.ToShortDateString(), type.ToString(), ipAddress + " " + Convert.ToString(contents) + " " + string.Format("{0}::{1}", callingObjectName, callingMethodName));
                    }
                }
                   
            }
            catch (Exception ex)
            {
                string FileName = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/");
                ErrLogs(FileName, System.DateTime.Now.ToShortDateString(), type.ToString(), ipAddress + " " + ex.Message + " " + ex.StackTrace + " " + ex.InnerException + " " + ex.GetType().ToString());
            }
           
        }

        #endregion

    }
}
