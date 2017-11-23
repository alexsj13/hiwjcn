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

namespace Lib.distributed.zookeeper
{
    /// <summary>
    /// 资料：邮箱搜索“zookeeper资料”
    /// </summary>
    public class ZooKeeperClient : IDisposable
    {
        protected bool IsDisposing = false;
        private ZooKeeper _client;

        protected readonly string _host;
        protected readonly TimeSpan _timeout;
        protected readonly Watcher _connection_status_watcher;

        private readonly ManualResetEvent _client_lock = new ManualResetEvent(false);
        private readonly object _create_client_lock = new object();

        public event Action OnConnected;
        public event Action OnUnConnected;
        public event Action<Exception> OnError;

        public ZooKeeperClient(ZooKeeperConfigSection configuration) :
            this(configuration.Server, TimeSpan.FromMilliseconds(configuration.SessionTimeOut))
        {
            //
        }

        public ZooKeeperClient(string host, TimeSpan? timeout = null)
        {
            this._host = host ?? throw new ArgumentNullException(nameof(host));
            this._timeout = timeout ?? TimeSpan.FromSeconds(30);
            this._connection_status_watcher = new ReconnectionWatcher(() =>
            {
                //服务可用
                this._client_lock.Set();
                this.OnConnected?.Invoke();
            }, () =>
            {
                //服务不可用
                this._client_lock.Reset();
                this.OnUnConnected?.Invoke();
            });

            //connect
            this.CreateClient();
        }

        protected virtual void CreateClient()
        {
            if (this._client == null)
            {
                lock (this._create_client_lock)
                {
                    if (this._client == null)
                    {
                        this._client = new ZooKeeper(
                            this._host,
                            (int)this._timeout.TotalMilliseconds,
                            this._connection_status_watcher);
                        //等待连上
                        this._client_lock.WaitOneOrThrow(TimeSpan.FromSeconds(30), "无法链接zk");
                    }
                }
            }
        }

        public bool IsAlive
        {
            get => this._client?.getState() == ZooKeeper.States.CONNECTED;
        }

        public ZooKeeper Client { get => this.GetClientManager(); }

        /// <summary>
        /// 等待可用链接，默认30秒超时
        /// </summary>
        /// <returns></returns>
        public virtual ZooKeeper GetClientManager(TimeSpan? timeout = null)
        {
            try
            {
                this._client_lock.WaitOneOrThrow(timeout ?? TimeSpan.FromSeconds(30));

                if (this._client == null) { throw new Exception("zookeeper client is not prepared"); }

                return this._client;
            }
            catch (KeeperException.ConnectionLossException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw e;
            }
            catch (KeeperException.SessionExpiredException e)
            {
                this.OnError?.Invoke(e);
                //链接断开
                throw e;
            }
            catch (Exception e)
            {
                this.OnError?.Invoke(e);
                throw e;
            }
        }

        protected virtual void CloseClient()
        {
            try
            {
                if (this._client != null)
                {
                    Task.Factory.StartNew(async () => await this._client.closeAsync()).Wait();
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            finally
            {
                this._client = null;
            }
        }

        public virtual void Dispose()
        {
            this.IsDisposing = true;

            this.CloseClient();
            this._client_lock.Dispose();
        }
    }
}
