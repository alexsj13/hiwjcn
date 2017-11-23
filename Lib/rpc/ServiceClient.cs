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
    /// wcf请求client，直接使用using就可以正确关闭链接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    {
        public ServiceClient()
        {
            //read config
        }

        public ServiceClient(Binding binding, EndpointAddress endpoint) : base(binding, endpoint)
        {
            //manual config
        }

        public ServiceClient(string url, bool http = true) :
            this(http ? new BasicHttpBinding()
            {
                MaxBufferSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                MaxReceivedMessageSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                ReceiveTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.ReceiveTimeoutSecond"] ?? "20").ToInt(null)),
                SendTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.SendTimeoutSecond"] ?? "20").ToInt(null))
            } : throw new Exception("暂不支持"), new EndpointAddress(url))
        {
            //with default config
        }

        class LogProxy : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();

                var valueType = invocation.ReturnValue;
            }
        }

        private T _ins = null;

        private readonly object _create_proxy_lock = new object();

        [Obsolete("无法代理channel，方法没有实现")]
        public bool UseProxy { get; set; } = false;

        /// <summary>
        /// 接口实例
        /// </summary>
        public T Instance
        {
            get
            {
                if (this.UseProxy)
                {
                    if (this._ins == null)
                    {
                        lock (this._create_proxy_lock)
                        {
                            if (this._ins == null)
                            {
                                var p = new ProxyGenerator();
                                var classToProxy = this.Channel.GetType();
                                var proxy_ins = (T)p.CreateClassProxy(classToProxy, new LogProxy());

                                this._ins = proxy_ins;
                            }
                        }
                    }
                    return this._ins;
                }
                else
                {
                    return this.Channel;
                }
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.SafeClose_();
        }
    }
    
    /// <summary>
    /// 直接使用，不用using
    /// </summary>
    public static class ServiceHelper<T> where T : class
    {
        /// <summary>
        /// 同步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperationResult<R> Invoke<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return client.Invoke(func);
            }
        }

        /// <summary>
        /// 同步转异步执行
        /// https://www.zhihu.com/question/56539006
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("使用Task.Run。调用时请确保是IO密集型操作，而不是计算密集型！具体参见此方法备注中的URL")]
        public static async Task<OperationResult<R>> InvokeSyncAsAsync<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeSyncAsAsync(func);
            }
        }

        /// <summary>
        /// 异步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<OperationResult<R>> InvokeAsync<R>(Func<T, Task<R>> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeAsync(func);
            }
        }
    }
}
