using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;
using System.Web;
using System.Reflection;
using Castle.DynamicProxy;
using System.ServiceModel.Description;

namespace Lib.rpc
{
    /// <summary>
    /// 请求客户端扩展
    /// </summary>
    public static class ServiceClientExtension
    {
        public static List<Type> FindServiceContracts(this Assembly ass) =>
            ass.GetTypes().Where(x => x.IsInterface && x.GetCustomAttributes<ServiceContract_Attribute>().Any()).ToList();

        public static void FindSvc(this Assembly ass)
        {
            var tps = ass.GetTypes();
            var contracts = tps.Where(x => x.IsInterface && x.GetCustomAttributes<ServiceContract_Attribute>().Any()).ToList();

            var svcTypes = new List<Type>();
            foreach (var con in contracts)
            {
                var impls = tps.Where(x => x.IsNormalClass() && x.IsAssignableTo_(con)).ToList();
                if (impls.Count <= 0) { continue; }
                if (impls.Count > 1) { throw new Exception("一个wcf contracts只能有一个实现"); }
                svcTypes.Add(impls.First());
            }
            var names = svcTypes.Select(x => $"{x.Name.ToLower()}.svc");
            var path = HttpContext.Current.Server.MapPath("~/");
            //从磁盘上找到svc文件
            Com.FindFiles(path, f =>
            {
                if (names.Contains(f.Name.ToLower()))
                {
                    //parse path
                }
            });
        }

        public static void SafeClose_<T>(this ClientBase<T> client) where T : class
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                try
                {
                    client.Abort();
                }
                catch (Exception err)
                {
                    err.AddErrorLog();
                }
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="client"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperationResult<R> Invoke<T, R>(this ServiceClient<T> client, Func<T, R> func) where T : class
        {
            try
            {
                return new OperationResult<R>() { Result = func.Invoke(client.Instance), Success = true };
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return new OperationResult<R>() { Ex = e, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// 同步转异步执行
        /// https://www.zhihu.com/question/56539006
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/
        /// </summary>
        [Obsolete("使用Task.Run。调用时请确保是IO密集型操作，而不是计算密集型！具体参见此方法备注中的URL")]
        public static async Task<OperationResult<R>> InvokeSyncAsAsync<T, R>(this ServiceClient<T> client, Func<T, R> func) where T : class
        {
            return await Task.Run(() => client.Invoke(func));
        }

        /// <summary>
        /// 异步执行
        /// </summary>
        public static async Task<OperationResult<R>> InvokeAsync<T, R>(this ServiceClient<T> client, Func<T, Task<R>> func) where T : class
        {
            try
            {
                var data = await func.Invoke(client.Instance);
                return new OperationResult<R>() { Result = data, Success = true };
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return await Task.FromResult(new OperationResult<R>() { Ex = e, ErrorMessage = e.Message });
            }
        }
    }
}
