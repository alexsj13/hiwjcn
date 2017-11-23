using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;

namespace Lib.mvc
{
    public static class MvcExtension
    {
        /// <summary>
        /// 获取Area Controller Action
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static (string area, string controller, string action) GetA_C_A(this RouteData route)
        {
            var AreaName = ConvertHelper.GetString(route.Values["Area"]);
            var ControllerName = ConvertHelper.GetString(route.Values["Controller"]);
            var ActionName = ConvertHelper.GetString(route.Values["Action"]);
            return (AreaName, ControllerName, ActionName);
        }

        /// <summary>
        /// 获取类似/home/index的url
        /// </summary>
        public static string ActionUrl(this RouteData route)
        {
            var data = route.GetA_C_A();
            var sp = new string[] { data.area, data.controller, data.action }.Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            if (!ValidateHelper.IsPlumpList(sp))
            {
                throw new Exception("无法获取action访问路径");
            }
            return "/" + "/".Join_(sp);
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        [Obsolete("用context扩展")]
        public static string Ip(this HttpRequest req)
        {
            return RequestHelper.GetCurrentIpAddress(req);
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        public static string Ip(this HttpContext context)
        {
            return RequestHelper.GetCurrentIpAddress(context.Request);
        }

        /// <summary>
        /// 是否是post
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsPost(this HttpRequest req)
        {
            return RequestHelper.IsPost(req);
        }

        /// <summary>
        /// 是否是ajax
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsAjax(this HttpRequest req)
        {
            return RequestHelper.IsAjax(req);
        }

        /// <summary>
        /// 获取根目录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpRequest req)
        {
            return RequestHelper.GetBaseUrl(req);
        }

        /// <summary>
        /// 获取当前访问地址
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string GetCurrentUrl(this HttpRequest req)
        {
            return RequestHelper.GetCurrentUrl(req);
        }

        /// <summary>
        /// 获取上传文件的字节数组
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this HttpPostedFile file)
        {
            var bs = IOHelper.GetPostFileBytesAndDispose(file);
            return bs;
        }

        /// <summary>
        /// 是否是SSL
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsSSL(this HttpRequest req)
        {
            return RequestHelper.IsSSL(req);
        }

        /// <summary>
        /// post和get数据的合并
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PostAndGet(this HttpContext context) =>
            context.QueryStringToDict().AddDict(context.PostToDict());

        /*
            var dict = context.Request.Form.ToDict();
            dict = dict.AddDict(context.Request.QueryString.ToDict());
            return dict;
             */

        /// <summary>
        /// get数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryStringToDict(this HttpContext context) =>
            context.Request.QueryString.ToDict();

        /// <summary>
        /// post数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PostToDict(this HttpContext context) =>
            context.Request.Form.ToDict();

        /// <summary>
        /// 获取这个程序集中所用到的所有权限
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static (List<string> permissions, List<string> scopes) ScanAllAssignedPermissionOnThisAssembly(this Controller controller)
        {
            var permission_list = new List<string>();
            var scope_list = new List<string>();
            var tps = controller.GetType().Assembly.GetTypes();
            tps = tps.Where(x => x.IsNormalClass() && x.IsAssignableTo_<Controller>()).ToArray();
            foreach (var t in tps)
            {
                var methods = t.GetMethods().Where(x => x.IsPublic);
                foreach (var m in methods)
                {
                    var attr = m.GetCustomAttribute<ValidLoginBaseAttribute>();
                    if (attr == null) { continue; }
                    var pers = attr.Permission?.Split(',').ToList();
                    if (ValidateHelper.IsPlumpList(pers))
                    {
                        permission_list.AddRange(pers);
                    }
                    var scopes = attr.Scope?.Split(',').ToList();
                    if (ValidateHelper.IsPlumpList(scopes))
                    {
                        scope_list.AddRange(scopes);
                    }
                }
            }
            permission_list = permission_list.Distinct().Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            scope_list = scope_list.Distinct().Where(x => ValidateHelper.IsPlumpString(x)).ToList();

            return (permission_list, scope_list);
        }
    }
}
