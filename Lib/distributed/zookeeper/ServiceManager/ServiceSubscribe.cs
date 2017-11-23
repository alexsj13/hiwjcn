using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using org.apache.zookeeper;
using Lib.extension;
using Lib.helper;
using Lib.data;
using Lib.ioc;
using Lib.core;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;
using org.apache.zookeeper.data;
using System.Net;
using System.Net.Http;
using Lib.net;
using Lib.rpc;
using Lib.distributed.zookeeper.watcher;
using Polly;
using System.Text;

namespace Lib.distributed.zookeeper.ServiceManager
{
    /// <summary>
    /// 应该作为静态类
    /// </summary>
    public class ServiceSubscribe : ServiceManageBase
    {
        private readonly Watcher _children_watcher;
        private readonly Watcher _node_watcher;
        private readonly List<AddressModel> _endpoints = new List<AddressModel>();
        private readonly Random _ran = new Random((int)DateTime.Now.Ticks);

        public event Action OnServiceChanged;

        public ServiceSubscribe(string host) : base(host)
        {
            this._children_watcher = new CallBackWatcher(e =>
            {
                return this.WatchNodeChanges(e);
            });
            this._node_watcher = new CallBackWatcher(e =>
            {
                return this.WatchNodeChanges(e);
            });

            this.StartWatch();
            this.OnConnected += () => this.StartWatch();
        }

        public IReadOnlyList<AddressModel> AllService() => this._endpoints.AsReadOnly();

        public AddressModel Resolve<T>()
        {
            var name = ServiceManageHelper.ParseServiceName<T>();
            var list = this._endpoints.Where(x => x.ServiceNodeName == name).ToList();
            if (ValidateHelper.IsPlumpList(list))
            {
                var theone = this._ran.Choice(list);
                Console.WriteLine($"选择了地址：{theone.Url}");
                return theone;
            }
            return null;
        }

        public string ResolveSvc<T>() => this.Resolve<T>()?.Url;

        private void StartWatch()
        {
            try
            {
                this.Retry().Execute(() =>
                {
                    AsyncHelper_.RunSync(() => this.NodeChildrenChanged(this._base_path));
                });
            }
            catch (Exception e)
            {
                throw new Exception("订阅服务节点失败", e);
            }
        }

        private async Task NodeChildrenChanged(string path)
        {
            try
            {
                var path_level = path.SplitZookeeperPath().Count;

                if (path_level == this._base_path_level)
                {
                    //qpl/wcf
                    var services = await this.Client.GetChildrenOrThrow_(path, this._children_watcher);
                    foreach (var service in services)
                    {
                        var endpoints = await this.Client.GetChildrenOrThrow_(path + "/" + service, this._children_watcher);
                        foreach (var endpoint in endpoints)
                        {
                            //处理节点
                            await this.HandleEndpointNode(path + "/" + service + "/" + endpoint);
                        }
                    }
                }
                else if (path_level == this._service_path_level)
                {
                    //qpl/wcf/order
                    var endpoints = await this.Client.GetChildrenOrThrow_(path, this._children_watcher);
                    foreach (var endpoint in endpoints)
                    {
                        //处理节点
                        await this.HandleEndpointNode(path + "/" + endpoint);
                    }
                }
                else
                {
                    $"不能处理的节点{path}".AddBusinessInfoLog();
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        private async Task HandleEndpointNode(string path)
        {
            if (!this.IsEndpointLevel(path)) { return; }
            try
            {
                await EndpointDataChanged(path);
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        private async Task EndpointDataChanged(string path)
        {
            if (!this.IsEndpointLevel(path)) { return; }
            try
            {
                var bs = await this.Client.GetDataOrThrow_(path, this._node_watcher);
                if (!ValidateHelper.IsPlumpList(bs))
                {
                    await this.Client.DeleteNodeRecursively_(path);
                    return;
                }
                var data = this._serializer.Deserialize<AddressModel>(bs);
                if (!ValidateHelper.IsAllPlumpString(data.Id, data.Url)) { return; }
                var service_info = this.GetServiceAndEndpointNodeName(path);
                data.ServiceNodeName = service_info.service_name;
                data.EndpointNodeName = service_info.endpoint_name;
                data.OnLineTime = DateTime.Now;

                this._endpoints.RemoveWhere_(x => x.Id == data.Id);
                this._endpoints.Add(data);
                this.OnServiceChanged?.Invoke();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }

        private async Task EndpointDeleted(string path)
        {
            if (!this.IsEndpointLevel(path)) { return; }
            var data = this.GetServiceAndEndpointNodeName(path);

            this._endpoints.RemoveWhere_(
                x => x.ServiceNodeName == data.service_name && x.EndpointNodeName == data.endpoint_name);

            this.OnServiceChanged?.Invoke();

            await Task.FromResult(1);
        }

        private async Task WatchNodeChanges(WatchedEvent e)
        {
            var event_type = e.get_Type();
            var path = e.getPath();

            var path_level = path.SplitZookeeperPath().Count;
            if (path_level < this._base_path_level || path_level > this._endpoint_path_level)
            {
                $"节点无法被处理{path}".AddBusinessInfoLog();
            }

            Console.WriteLine($"节点事件：{path}:{event_type}");

            switch (event_type)
            {
                case Watcher.Event.EventType.NodeChildrenChanged:
                    //子节点发生更改
                    await this.NodeChildrenChanged(path);
                    break;

                case Watcher.Event.EventType.NodeDataChanged:
                    //单个节点数据发生修改
                    await this.EndpointDataChanged(path);
                    break;
                case Watcher.Event.EventType.NodeDeleted:
                    await this.EndpointDeleted(path);
                    break;
                default:
                    break;
            }
        }

    }
}
