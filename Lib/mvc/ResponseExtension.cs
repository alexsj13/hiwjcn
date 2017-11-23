using Lib.helper;
using System;
using System.Configuration;
using System.Web;

namespace Lib.mvc
{
    public static class ResponseExtension
    {
        /// <summary>
        /// 设置不缓存
        /// </summary>
        /// <param name="response"></param>
        public static void SetResponseNoCache(this HttpResponse response)
        {
            response.Buffer = false;
            response.ExpiresAbsolute = DateTime.Now.AddMilliseconds(0);
            response.Expires = 0;
            response.CacheControl = "no-cache";
            response.AppendHeader("Pragma", "No-Cache");
        }

        public static void AllowCrossDomainAjax(this HttpContext context)
        {
            var Origin_Allow = ConvertHelper.GetString(ConfigurationManager.AppSettings["Origin_Allow"]).ToLower();
            if (!ValidateHelper.IsPlumpString(Origin_Allow)) { return; }
            //添加header实现跨域
            var Origin = ConvertHelper.GetString(context.Request.Headers["Origin"]);
            if (Origin.ToLower().IndexOf(Origin_Allow) >= 0)
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = Origin;
                context.Response.Headers["Access-Control-Allow-Headers"] = "*, Origin, X-Requested-With, X_Requested_With, Content-Type, Accept";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            }
        }
    }
}
