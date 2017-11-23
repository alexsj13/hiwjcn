using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.distributed.zookeeper.watcher
{
    public abstract class WatcherBase : Watcher
    {
        protected readonly string Path;

        protected WatcherBase(string path)
        {
            this.Path = path;
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            var event_type = watchedEvent.get_Type();
            var zk_status = watchedEvent.getState();
            var path = watchedEvent.getPath();

            if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != Path)
            {
                $"{watchedEvent.ToJson()}".AddBusinessInfoLog();
                return;
            }

            await ProcessImpl(watchedEvent);

            /*
                         switch (zk_status)
            {
                case Event.KeeperState.AuthFailed:
                    Console.WriteLine(nameof(Event.KeeperState.AuthFailed));
                    break;
                case Event.KeeperState.ConnectedReadOnly:
                    Console.WriteLine(nameof(Event.KeeperState.ConnectedReadOnly));
                    break;
                case Event.KeeperState.Disconnected:
                    Console.WriteLine(nameof(Event.KeeperState.Disconnected));
                    this.ReConnect();
                    break;
                case Event.KeeperState.Expired:
                    Console.WriteLine(nameof(Event.KeeperState.Expired));
                    this.ReConnect();
                    break;
                case Event.KeeperState.SyncConnected:
                    break;
                default:
                    break;
            }

            switch (event_type)
            {
                case Event.EventType.NodeChildrenChanged:
                    //注册节点发生改变
                    await this.NodeChildrenChanged(path);
                    break;
                case Event.EventType.NodeCreated:
                    break;
                case Event.EventType.NodeDataChanged:
                    break;
                case Event.EventType.NodeDeleted:
                    break;
                case Event.EventType.None:
                    break;
                default:
                    break;
            }
             */
        }

        protected abstract Task ProcessImpl(WatchedEvent watchedEvent);
    }
}
