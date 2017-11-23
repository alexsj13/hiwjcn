using Lib.extension;
using Lib.net;
using Lib.task;
using Lib.helper;
using Quartz;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lib.core;
using Lib.ioc;
using Lib.data;
using Polly;
using Hiwjcn.Core.Domain.WCF;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class WcfHeartBeatTask : QuartzJobBase
    {
        public override string Name => "WCF服务心跳检测";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInSeconds(15);

        private void PushToQueue(ref Queue queue)
        {
            using (var s = AppContext.Scope())
            {
                var repo = s.Resolve_<IRepository<WcfMap>>();
                var max_iid = 0.00;
                while (true)
                {
                    var list = repo.QueryList(where: x => x.IID > max_iid, orderby: x => x.IID, Desc: false, count: 200);
                    if (!ValidateHelper.IsPlumpList(list))
                    {
                        //生产完成
                        return;
                    }
                    foreach (var x in list)
                    {
                        queue.Enqueue(x);
                    }
                    max_iid = list.Max(x => x.IID);
                }
            }
        }

        private void ConsumeQueue(ref Queue queue, Func<bool> stop)
        {
            while (!stop.Invoke())
            {
                var model = default(WcfMap);
                try
                {
                    lock (queue)
                    {
                        if (queue.Count <= 0)
                        {
                            //队列为空，等一会儿
                            Thread.Sleep(200);
                            continue;
                        }
                        model = (queue.Dequeue() as WcfMap) ?? throw new Exception("queue中拿出的数据为null");
                    }
                    //request url and update last update time

                    var ok = false;
                    try
                    {
                        Policy.Handle<Exception>()
                            .WaitAndRetry(3, i => TimeSpan.FromMilliseconds(i * 100))
                            .Execute(() =>
                        {
                            HttpClientHelper.Send(model.SvcUrl,
                                method: RequestMethodEnum.GET,
                                ensure_http_code: new int[] { 200 });
                        });
                        ok = true;
                    }
                    catch (Exception e)
                    {
                        ok = false;
                        e.AddErrorLog(this.Name);
                    }
                    using (var s = AppContext.Scope())
                    {
                        var repo = s.Resolve_<IRepository<WcfMap>>();
                        var m = repo.GetFirst(x => x.IID == model.IID);
                        if (ok)
                        {
                            m.Update();
                            m.HeartBeatsTime = m.UpdateTime;
                            repo.Update(m);
                        }
                        else
                        {

                            repo.Delete(m);
                        }
                    }
                }
                catch (Exception e)
                {
                    e.AddErrorLog($"{this.Name}:wcfmap对象：{model?.ToJson()}");
                    Thread.Sleep(200);
                }
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            var s = true;
            if (s)
            {
                return;
            }

            Action<long, string> logger = (ms, name) =>
            {
                $"{this.Name},耗时：{ms}毫秒".AddBusinessInfoLog();
            };
            using (var timer = new CpuTimeLogger(logger))
            {
                var stop = false;
                var thread_count = 5;
                var queue = new Queue();
                //add task to queue

                Thread CreateThread() => new Thread(() => this.ConsumeQueue(ref queue, () => stop)) { IsBackground = true };
                var tts = Com.Range(thread_count).Select(x => CreateThread());
                var thread_list = new List<Thread>(tts);

                try
                {
                    //开启消费
                    thread_list.ForEach(t => t.Start());

                    //开启生产
                    this.PushToQueue(ref queue);
                }
                catch (Exception e)
                {
                    e.AddErrorLog(this.Name);
                }
                finally
                {
                    //生产结束，消费可以结束
                    stop = true;
                    //等待结束
                    thread_list.ForEach_(t => t.Join());
                    thread_list.Clear();
                }
            }
        }
    }
}
