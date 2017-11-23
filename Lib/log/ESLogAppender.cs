using Lib.data;
using log4net.Appender;
using log4net.Core;
using System;
using Lib.extension;
using System.Collections.Generic;
using Nest;
using System.Diagnostics;
using Lib.helper;
using System.Linq;
using Polly;
using Polly.CircuitBreaker;
using Akka;
using Akka.Actor;
using Lib.events;
using Lib.distributed.akka;
using Lib.data.elasticsearch;

namespace Lib.log
{
    /// <summary>
    /// 使用redis存储日志
    /// https://github.com/lokki/RedisAppender
    /// </summary>
    public class ESLogAppender : BufferingAppenderSkeleton
    {
        private readonly IActorRef WriterActor;
        private readonly Random ran;

        private readonly int ThreadHold = 10;

        public ESLogAppender()
        {
            var sys = AkkaSystemManager.Instance.DefaultClient;
            this.WriterActor = sys.CreateActor<SendLogActor>();
            this.ran = new Random((int)DateTime.Now.Ticks);
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (this.BufferSize <= 0) { throw new Exception($"{nameof(this.BufferSize)}必须大于0"); }
            if (this.BufferSize < this.ThreadHold)
            {
                Debug.WriteLine($"警告：ES Appender的{nameof(this.BufferSize)}小于{this.ThreadHold}，负载大的时候容易造成{nameof(this.WriterActor)}内存溢出");
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            try
            {
                if (!ValidateHelper.IsPlumpList(events)) { return; }

                //缓冲设置的越小，阻塞的几率越大==========暂时不开启
                if (this.BufferSize < this.ThreadHold && false)
                {
                    //等待的概率
                    var AskProbability = 1 - ((double)this.BufferSize) / this.ThreadHold;

                    var r = ran.RealNext(this.ThreadHold);
                    if (r < AskProbability * this.ThreadHold)
                    {
                        //等待结束
                        var task = this.WriterActor.Ask<bool>(events);
                        Lib.helper.AsyncHelper.RunSync(() => task);
                        return;
                    }
                }
                //不等待结束
                this.WriterActor.Tell(events);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetInnerExceptionAsJson());
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
        }
    }

    public class SendLogActor : ReceiveActor
    {
        private static readonly CircuitBreakerPolicy p =
            Policy.Handle<Exception>().CircuitBreaker(100, TimeSpan.FromMinutes(1));

        public SendLogActor()
        {
            var pool = ElasticsearchClientManager.Instance.DefaultClient;
            var client = pool.CreateClient();
            client.CreateIndexIfNotExists(ESLogHelper.IndexName);

            this.Receive<LoggingEvent[]>(events =>
            {
                try
                {
                    //错误熔断
                    p.Execute(() =>
                    {
                        client.AddToIndex(ESLogHelper.IndexName, events.Select(x => new ESLogLine(x)).ToArray());
                    });
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.GetInnerExceptionAsJson());
                }
                this.Sender.Tell(true);
            });
        }
    }
}
