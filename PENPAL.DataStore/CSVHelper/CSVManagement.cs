using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel;
using Microsoft.VisualBasic.FileIO;

namespace PENPAL.DataStore.CSVHelper
{
    public class CSVManagement
    {
        private static char _separator = ',';

        public static char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        /// <summary>
        /// Write Data into CSV formatted String
        /// </summary>
        /// <param name="list">List of data in the form of Dictionary Expando Object </param>
        /// <param name="fileName">File Name</param>
        public static void Serialize(List<object> list, string fileName)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetHeaders(list));
                var values = new List<string>();
                foreach (IEnumerable dataList in (IEnumerable)list)
                {
                    values.Clear();
                    foreach (dynamic data in dataList)
                    {
                        if (data.Value != null)
                            values.Add("\"" + data.Value.ToString().Trim() + "\"");
                        else
                            values.Add("");
                    }
                    sb.AppendLine(string.Join(Separator.ToString(), values.ToArray()));
                }
                DownloadCSV(sb.ToString(), fileName);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string Serialize(List<object> list)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetHeaders(list));
                var values = new List<string>();
                foreach (IEnumerable dataList in (IEnumerable)list)
                {
                    values.Clear();
                    foreach (dynamic data in dataList)
                    {
                        if (data.Value != null)
                            values.Add("\"" + data.Value.ToString().Trim() + "\"");
                        else
                            values.Add("");
                    }
                    sb.AppendLine(string.Join(Separator.ToString(), values.ToArray()));
                }
                return sb.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Get Headers of CSV
        /// </summary>
        /// <param name="list">List of dynamic Data</param>
        /// <returns>Header String</returns>
        private static string GetHeaders(List<object> list)
        {
            try
            {
                var dataList = (IEnumerable)list[0];
                List<string> headerList = new List<string>();
                foreach (dynamic data in dataList)
                {
                    headerList.Add(data.Key);
                }
                return string.Join(Separator.ToString(), headerList.ToArray());
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// Download CSV file using HttpContext
        /// </summary>
        /// <param name="csvContent">Csv File Content</param>
        /// <param name="fileName">File Name</param>
        private static void DownloadCSV(string csvContent, string fileName)
        {
            try
            {
                string attachment = "attachment; filename=" + fileName + "";
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                HttpContext.Current.Response.Charset = System.Text.Encoding.UTF8.EncodingName;
                HttpContext.Current.Response.ContentType = "text/csv";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Unicode;
                HttpContext.Current.Response.AddHeader("Pragma", "public");
                HttpContext.Current.Response.Write(csvContent);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns></returns>
        public static IList<object> Deserialize(Stream stream)
        {
            string[] columns;
            string[] rows;
            try
            {
                List<object> deserialisedDynamicDataList = new List<object>();
                try
                {
                    using (var sr = new StreamReader(stream))
                    {

                        columns = sr.ReadLine().Split(Separator);
                        rows = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                for (var row = 0; row < rows.Length; row++)
                {
                    var dynamicProperties = new ExpandoObject() as IDictionary<string, Object>;
                    var line = rows[row];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }


                    TextFieldParser parser = new TextFieldParser(new StringReader(line));

                    // You can also read from a file
                    // TextFieldParser parser = new TextFieldParser("mycsvfile.csv");

                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    string[] parts = null;

                    while (!parser.EndOfData)
                    {
                        parts = parser.ReadFields();
                    }
                    parser.Close();
                    //var parts = line.Split(Separator);

                    for (var iCnt = 0; iCnt < parts.Length; iCnt++)
                    {
                        var column = columns[iCnt];
                        var value = parts[iCnt];
                        dynamicProperties.Add(column, value);
                    }

                    deserialisedDynamicDataList.Add(dynamicProperties);
                }

                return deserialisedDynamicDataList;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
