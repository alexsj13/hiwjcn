using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Lib.distributed.zookeeper;
using Polly;
using Lib.rpc;
using Lib.helper;
using org.apache.zookeeper;
using Lib.extension;

namespace Lib.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceRegister : ServiceManageBase
    {
        private readonly string _base_url;
        private readonly string _node_id;
        private readonly IReadOnlyList<Assembly> _ass;
        
        public ServiceRegister(string host, string service_base_url, string node_id, Assembly[] ass) : base(host)
        {
            this._base_url = service_base_url ?? throw new ArgumentNullException(nameof(service_base_url));
            this._node_id = node_id ?? throw new ArgumentNullException(nameof(node_id));
            this._ass = (ass ?? throw new ArgumentNullException(nameof(ass))).ToList().AsReadOnly();
            if (this._ass.Count <= 0) { throw new Exception("至少注册一个程序集"); }

            this.Retry().Execute(() => this.Reg());
            this.OnRecconected += () => this.Reg();
        }

        public void Reg() => AsyncHelper_.RunSync(() => this.RegisterService());

        private async Task RegisterService()
        {
            var now = DateTime.Now;
            var list = new List<AddressModel>();

            foreach (var a in this._ass)
            {
                foreach (var t in a.FindServiceContracts())
                {
                    var attr = t.GetCustomAttributes_<ServiceContract_Attribute>().FirstOrDefault() ??
                        throw new Exception("不应该出现的错误");
                    var model = new AddressModel()
                    {
                        Id = this._node_id,
                        Url = this._base_url + attr.RelativePath,
                        ServiceNodeName = ServiceManageHelper.ParseServiceName(t),
                        EndpointNodeName = ServiceManageHelper.EndpointNodeName(this._node_id),
                        OnLineTime = now,
                    };
                    list.Add(model);
                }
            }

            foreach (var m in list)
            {
                var service_path = this._base_path + "/" + m.ServiceNodeName;
                await this.Client.EnsurePath(service_path);

                var path = service_path + "/" + m.EndpointNodeName;
                var data = this._serializer.Serialize(m);
                if (await this.Client.ExistAsync_(path))
                {
                    await this.Client.SetDataAsync_(path, data);
                }
                else
                {
                    await this.Client.CreateNode_(path, CreateMode.EPHEMERAL, data);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
