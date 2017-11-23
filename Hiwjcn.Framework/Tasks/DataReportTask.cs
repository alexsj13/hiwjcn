using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class DataReportTask : QuartzJobBase
    {
        public override string Name
        {
            get
            {
                return "汇报统计数据";
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
                //每天早上6点发送昨天的数据报告
                return TriggerDaily(6, 0);
            }
        }

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                var start = DateTime.Now;
                //do something
                Thread.Sleep(new Random((int)DateTime.Now.Ticks).Next(10) * 1000);
                var end = DateTime.Now;
                $"{start}开始汇报数据，{end}结束。耗时：{(end - start).TotalSeconds}秒".AddBusinessInfoLog();
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
