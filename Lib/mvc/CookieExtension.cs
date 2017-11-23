using Lib.helper;
using System;
using System.Collections.Generic;
using System.Web;

namespace Lib.mvc
{
    /// <summary>
    /// 操作cookie
    /// </summary>
    public static class CookieExtension
    {
        /// <summary>
        /// 移除cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="domain"></param>
        public static void RemoveCookie(this HttpContext context, string name, string domain = null)
        {
            RemoveCookie(context, new string[] { name }, domain: domain);
        }

        /// <summary>
        /// 删除cookie
        /// </summary>
        public static void RemoveCookie(this HttpContext context, string[] names, string domain = null)
        {
            if (!ValidateHelper.IsPlumpList(names)) { throw new ArgumentException(); }
            foreach (var name in names)
            {
                SetCookie(context, name, string.Empty, domain: domain, expires_minutes: -60 * 24);
            }
            /*
            foreach (var name in names)
            {
                var cookie = context.Request.Cookies[name];
                if (cookie == null)
                {
                    continue;
                }
                if (ValidateHelper.IsPlumpString(domain))
                {
                    //设为顶级域名才可以在二级域名中删除顶级域名的cookie
                    //比如要在blog.xx.com中删除.xx.com的cookie，就需要设置domain为.xx.com
                    cookie.Domain = domain;
                }
                cookie.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(cookie);
                //remove方法只是不让服务器向客户机发送那个被删除的cookie，与此cookie留不留在客户机里无关。
                //remove无法删除客户端的cookie，需要设置过期时间
                //context.Response.Cookies.Remove("");
            }*/
        }
        /// <summary>
        /// 移除当前response上下文的cookie，不会删除浏览器cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="names"></param>
        public static void RemoveResponseCookies(this HttpContext context, string[] names)
        {
            foreach (var name in names)
            {
                var cookie = context.Request.Cookies[name];
                if (cookie == null) { continue; }
                context.Request.Cookies.Remove(name);
            }
        }
        /// <summary>
        /// 读取cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCookie(this HttpContext context, string name, string key = null)
        {
            var cookie = context.Request.Cookies[name];
            if (cookie == null) { return string.Empty; }

            if (ValidateHelper.IsPlumpString(key))
            {
                if (cookie.HasKeys)
                {
                    return ConvertHelper.GetString(cookie[key]);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return ConvertHelper.GetString(cookie.Value);
            }
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="httpOnly"></param>
        /// <param name="expires_minutes"></param>
        public static void SetCookie(this HttpContext context, string name,
            string value, Dictionary<string, string> values = null,
            string domain = null, string path = null, bool? httpOnly = null,
            double expires_minutes = 60 * 24)
        {
            context.Request.Cookies.Remove(name);
            //直接创建新cookie，覆盖本地cookie或者使用过期来删除本地cookie
            var cookie = context.Request.Cookies[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name);
            }
            cookie = new HttpCookie(name);
            if (ValidateHelper.IsPlumpDict(values))
            {
                foreach (var key in values.Keys)
                {
                    cookie[key] = values[key];
                }
            }
            else
            {
                cookie.Value = value;
            }
            if (ValidateHelper.IsPlumpString(domain)) { cookie.Domain = domain; }
            if (ValidateHelper.IsPlumpString(path)) { cookie.Path = path; }
            if (httpOnly != null) { cookie.HttpOnly = httpOnly.Value; }
            cookie.Expires = DateTime.Now.AddMinutes(expires_minutes);
            context.Response.Cookies.Add(cookie);
        }

    }
}
