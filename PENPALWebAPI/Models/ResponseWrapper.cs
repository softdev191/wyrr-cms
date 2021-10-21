using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace PENPALWebAPI.Models
{
    public class ResponseWrapper<T> where T : class
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
        public List<T> Result { get; set; }
        public bool IsNextPageExists { get; set; }
        public string NextPageLink { get; set; }

        /// <summary>
        /// ResponseWrapper is responsible to send response to consumer.
        /// </summary>
        /// <param name="lstItems"></param>
        /// <param name="isNextPagePresent"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorDetails"></param>
        public ResponseWrapper(List<T> lstItems, bool isNextPageExists, HttpStatusCode statusCode, string StatusDescription, bool responseStatus, string errorDetails, string nextPageLink)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = StatusDescription;
            this.ResponseStatus = responseStatus;
            this.Message = errorDetails;
            this.IsNextPageExists = isNextPageExists;
            this.NextPageLink = nextPageLink;

            if (lstItems != null)
                Result = lstItems;
            else
                Result = null;
        }
    }

    public class ResponseWrapperObject<T> where T : class
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
        public bool IsNextPageExists { get; set; }
        public string NextPageLink { get; set; }

        /// <summary>
        /// ResponseWrapper is responsible to send response to consumer.
        /// </summary>
        /// <param name="lstItems"></param>
        /// <param name="isNextPagePresent"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorDetails"></param>
        public ResponseWrapperObject(object items, bool isNextPageExists, HttpStatusCode statusCode, string StatusDescription, bool responseStatus, string errorDetails, string nextPageLink)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = StatusDescription;
            this.ResponseStatus = responseStatus;
            this.Message = errorDetails;
            this.IsNextPageExists = isNextPageExists;
            this.NextPageLink = nextPageLink;

            if (items != null)
                Result = items;
            else
                Result = null;
        }
    }

    public class DeviceResponseWrapper<T> where T : class
    {
        public List<T> Results { get; set; }
        public string PrefixPath { get; set; }
        public DateTime? LastSYNCDateTime { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Device Response Wrapper is responsible to send response to Device/Box.
        /// </summary>
        /// <param name="lstItems"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorDetails"></param>
        /// <param name="lastSYNCDateTime"></param>
        public DeviceResponseWrapper(List<T> lstItems, HttpStatusCode statusCode, string prefixPath = "", DateTime? lastSYNCDateTime = null)
        {
            this.StatusCode = statusCode;
            this.LastSYNCDateTime = lastSYNCDateTime;
            this.PrefixPath = prefixPath;

            if (lstItems != null)
                Results = lstItems;
            else
                Results = null;
        }
    }

    public class ResponseWrapperForAddUpdate<T> where T : class
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
        public List<T> Result { get; set; }
        // public bool IsNextPageExists { get; set; }
        // public string NextPageLink { get; set; }

        /// <summary>
        /// ResponseWrapper is responsible to send response to consumer.
        /// </summary>
        /// <param name="lstItems"></param>
        /// <param name="isNextPagePresent"></param>
        /// <param name="statusCode"></param>
        /// <param name="errorDetails"></param>
        public ResponseWrapperForAddUpdate(List<T> lstItems, HttpStatusCode statusCode, bool responseStatus,string StatusDescription, string errorDetails)
        {
            this.StatusCode = statusCode;
            this.ResponseStatus = responseStatus;
            this.Message = errorDetails;
            this.StatusDescription = StatusDescription;
            if (lstItems != null)
                Result = lstItems;
            else
                Result = null;

            //ReturnObject.Add(new ReponseDetails(false,))
        }
    }
}