using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.distributed;
using Lib.distributed.zookeeper;
using System.Reflection;
using Lib.extension;
using Polly;
using Lib.core;
using Lib.data;
using Lib.helper;

namespace Lib.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// /QPL/WCF/ORDER/m-1
    /// /QPL/WCF/ORDER/m-2
    /// /QPL/WCF/ORDER/m-3
    /// /QPL/WCF/ORDER/m-4
    /// </summary>
    public abstract class ServiceManageBase : AlwaysOnZooKeeperClient
    {
        protected readonly string _base_path;
        protected readonly int _base_path_level;
        protected readonly int _service_path_level;
        protected readonly int _endpoint_path_level;
        
        protected readonly SerializeHelper _serializer = new SerializeHelper();

        public ServiceManageBase(string host) : this(host, "/QPL/WCF") { }

        public ServiceManageBase(string host, string path) : base(host)
        {
            this._base_path = path ?? throw new ArgumentNullException(path);
            if (!this._base_path.StartsWith("/") || this._base_path.EndsWith("/"))
            {
                throw new Exception("path必须以/开头，并且不能以/结尾");
            }
            this._base_path_level = this._base_path.SplitZookeeperPath().Count;
            this._service_path_level = this._base_path_level + 1;
            this._endpoint_path_level = this._service_path_level + 1;

            try
            {
                this.Retry().Execute(() => this.InitBasePath());
            }
            catch (Exception e)
            {
                throw new Exception("尝试创建服务注册base path失败", e);
            }
        }

        protected void InitBasePath()
        {
            var client = this.GetClientManager();
            Task.Factory.StartNew(async () =>
            {
                await client.CreatePersistentPathIfNotExist_(this._base_path);
            }).Wait();
        }

        protected Policy Retry() => ServiceManageHelper.RetryPolicy();

        protected Policy RetryAsync() => ServiceManageHelper.RetryAsyncPolicy();

        protected bool IsServiceLevel(string path) =>
            path.SplitZookeeperPath().Count == this._service_path_level;

        protected bool IsEndpointLevel(string path) =>
            path.SplitZookeeperPath().Count == this._endpoint_path_level;

        protected (string service_name, string endpoint_name) GetServiceAndEndpointNodeName(string path)
        {
            if (!this.IsEndpointLevel(path)) { throw new Exception("只有终结点才能获取服务和节点信息"); }

            var data = path.SplitZookeeperPath().Reverse_();
            var endpoint_name = data.Take(1).FirstOrDefault();
            var service_name = data.Skip(1).Take(1).FirstOrDefault();
            return (service_name, endpoint_name);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
