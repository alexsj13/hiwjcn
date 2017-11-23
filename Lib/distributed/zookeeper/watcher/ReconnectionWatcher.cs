using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.watcher
{
    public class ReconnectionWatcher : Watcher
    {
        private readonly Action _connectioned;
        private readonly Action _disconnect;

        public ReconnectionWatcher(Action connectioned, Action disconnect)
        {
            this._connectioned = connectioned ?? throw new ArgumentNullException(nameof(connectioned));
            this._disconnect = disconnect ?? throw new ArgumentNullException(nameof(disconnect));
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            var status = watchedEvent.getState();
            if (status == Event.KeeperState.SyncConnected)
            {
                this._connectioned.Invoke();
            }
            else
            {
                this._disconnect.Invoke();
            }
            await Task.FromResult(1);
        }
    }
}
