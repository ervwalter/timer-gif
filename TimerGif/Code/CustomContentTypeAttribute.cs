using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimerImage
{
    public class CustomContentTypeAttribute : ActionFilterAttribute
    {
        public CustomContentTypeAttribute(string contentType)
        {
            ContentType = contentType;
            Order = 2;
        }

        public string ContentType { get; set; }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {

            filterContext.HttpContext.Response.ContentType = ContentType;

        }
    }
}