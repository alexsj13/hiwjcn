using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Lib.distributed.zookeeper.watcher
{
    public class NodeMonitorWatcher : WatcherBase
    {
        public NodeMonitorWatcher(string path) : base(path)
        { }

        protected override Task ProcessImpl(WatchedEvent watchedEvent)
        {
            throw new NotImplementedException();
        }
    }
}
