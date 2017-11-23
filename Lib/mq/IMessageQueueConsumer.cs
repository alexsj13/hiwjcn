using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mq
{
    public interface IMessageQueueConsumer : IMessageQueueConsumer<string> { }

    public interface IMessageQueueConsumer<T> : IDisposable
    {
        //
    }
}
