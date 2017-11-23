using Lib.helper;
using System;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Hosting;
using Lib.extension;
using System.Threading.Tasks;
using Lib.cache;

namespace Lib.mvc
{
    public static class ServerHelper
    {
        public static string AppDataPath(this HttpServerUtility server) => server.MapPath("~/App_Data");

        /// <summary>
        /// 在请求上下文中缓存对象,不能缓存null对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T CacheInHttpContext<T>(this HttpContext context, string key, Func<T> func)
        {
            if (context.Items.Contains(key))
            {
                var obj = context.Items[key];
                if (obj != null && obj is CacheResult<T> data)
                {
                    return data.Result;
                }
                else
                {
                    return default(T);
                }
            }
            var d = func.Invoke();
            context.Items[key] = new CacheResult<T>() { Result = d, Success = true };
            return d;
        }

        public static async Task<T> CacheInHttpContextAsync<T>(this HttpContext context, string key, Func<Task<T>> func)
        {
            if (context.Items.Contains(key))
            {
                var obj = context.Items[key];
                if (obj != null && obj is CacheResult<T> data)
                {
                    return data.Result;
                }
                else
                {
                    return default(T);
                }
            }
            var d = await func.Invoke();
            if (d != null)
            {
                context.Items[key] = new CacheResult<T>() { Result = d, Success = true };
            }
            return d;
        }

        /// <summary>
        /// 是否是服务器环境
        /// </summary>
        /// <returns></returns>
        public static bool IsHosted() => HostingEnvironment.IsHosted;

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMapPath(HttpContext context, string path)
        {
            try
            {
                return context.Server.MapPath(path);
            }
            catch
            {
                return MapPath(path);
            }
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMapPath(string path)
        {
            return GetMapPath(HttpContext.Current, path);
        }

        /// <summary>
        /// 来自nopcommerce的方法Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        private static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }
            //not hosted. For example, run in unit tests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }

        /// <summary>
        /// 获得当前应用程序的信任级别
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            AspNetHostingPermissionLevel trustLevel = AspNetHostingPermissionLevel.None;
            //权限列表
            AspNetHostingPermissionLevel[] levelList = new AspNetHostingPermissionLevel[] {
                                                                                            AspNetHostingPermissionLevel.Unrestricted,
                                                                                            AspNetHostingPermissionLevel.High,
                                                                                            AspNetHostingPermissionLevel.Medium,
                                                                                            AspNetHostingPermissionLevel.Low,
                                                                                            AspNetHostingPermissionLevel.Minimal
                                                                                            };

            foreach (AspNetHostingPermissionLevel level in levelList)
            {
                try
                {
                    //通过执行Demand方法检测是否抛出SecurityException异常来设置当前应用程序的信任级别
                    new AspNetHostingPermission(level).Demand();
                    trustLevel = level;
                    break;
                }
                catch (SecurityException ex)
                {
                    continue;
                }
            }
            return trustLevel;
        }


        /// <summary>
        /// 重启应用程序
        /// </summary>
        public static void RestartAppDomain(HttpContext context)
        {
            try
            {
                if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
                {
                    //如果当前信任级别大于Medium，则通过卸载应用程序域的方式重启
                    HttpRuntime.UnloadAppDomain();
                }
                else
                {
                    //通过修改web.config方式重启应用程序
                    File.SetLastWriteTime(ServerHelper.GetMapPath(context, "~/Web.config"), DateTime.Now);
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
