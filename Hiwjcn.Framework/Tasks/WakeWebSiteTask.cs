using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class WakeWebSiteTask : QuartzJobBase
    {
        public override string Name
        {
            get
            {
                return "唤醒网站";
            }
        }

        public override bool AutoStart
        {
            get
            {
                return true;
            }
        }

        public override ITrigger Trigger
        {
            get
            {
                return this.TriggerIntervalInSeconds(30);
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            var wakeuplist = new string[]
            {
                "http://auth.qipeilong.net",
                "http://auth.qipeilong.cn"
            };

            foreach (var url in wakeuplist)
            {
                try
                {
                    var html = HttpClientHelper.Get(url);
                    $"唤醒网站{url}，读取内容长度为{html?.Length}".AddBusinessInfoLog();
                }
                catch (Exception e)
                {
                    e.AddErrorLog($"{this.Name}:{url}");
                }
            }
        }
    }
}
