using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.watcher
{
    public class CallBackWatcher : Watcher
    {
        private readonly Func<WatchedEvent, Task> callback;

        public CallBackWatcher(Func<WatchedEvent, Task> callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override async Task process(WatchedEvent @event)
        {
            await this.callback.Invoke(@event);
        }
    }
}
