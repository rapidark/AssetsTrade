﻿using LooWooTech.AssetsTrade.Common;
using LooWooTech.AssetsTrade.Managers;
using LooWooTech.AssetsTrade.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LooWooTech.AssetsTrade.WebApi
{
    [UserAuthorize]
    public class ControllerBase : AsyncController
    {
        protected static ManagerCore Core = ManagerCore.Instance;
        protected UserIdentity CurrentUser
        {
            get { return Thread.CurrentPrincipal.Identity as UserIdentity; }
        }

        private static string _serializeType = System.Configuration.ConfigurationManager.AppSettings["SerializeType"] ?? "xml";
        private string GetSerializedContent(object data)
        {
            if (_serializeType == "json")
            {
                return data.JsonSerialize();
            }
            return data.XmlSerialize();
        }

        protected ActionResult ContentResult<T>(T data)
        {
            return new ContentResult { Content = GetSerializedContent(data), ContentEncoding = System.Text.Encoding.UTF8, ContentType = "text/" + _serializeType };
        }

        protected ActionResult SuccessResult<T>(T data) where T : class
        {
            if (data == null)
            {
                return ContentResult(new SuccessResult { Code = 1 });
            }
            return ContentResult(data);
        }

        protected ActionResult SuccessResult()
        {
            return ContentResult(new SuccessResult { Code = 1 });
        }

        protected ActionResult ErrorResult(string message)
        {
            return ContentResult(new ErrorResult { Code = 0, Message = message });
        }

        protected ActionResult ErrorResult(Exception ex)
        {
            return ContentResult(new ErrorResult { Code = 0, Message = ex.Message, StackTrace = ex.StackTrace });
        }

        private int GetStatusCode(Exception ex)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            if (ex is HttpException)
            {
                statusCode = (ex as HttpException).GetHttpCode();
            }
            else if (ex is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            return statusCode;
        }

        private Exception GetException(Exception ex)
        {
            var innerEx = ex.InnerException;
            if (innerEx != null)
            {
                return GetException(innerEx);
            }
            return ex;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            if (filterContext.HttpContext.Response.StatusCode == 200)
            {
                filterContext.HttpContext.Response.StatusCode = GetStatusCode(filterContext.Exception);
            }
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            var ex = GetException(filterContext.Exception);
            //如果是吊用接口时网络失败
            if(filterContext.Exception.Message.Contains("网络连接失败"))
            {

            }
            filterContext.Result = ErrorResult(ex);
        }
    }
}