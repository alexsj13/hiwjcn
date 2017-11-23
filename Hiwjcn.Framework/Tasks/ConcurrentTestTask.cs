using Lib.task;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using System.Threading;
using Lib.extension;
using Lib.helper;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ConcurrentTestTask : QuartzJobBase
    {
        public override bool AutoStart
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                return "测试任务并发";
            }
        }

        public override ITrigger Trigger
        {
            get
            {
                return this.TriggerIntervalInSeconds(1);
            }
        }

        /// <summary>
        /// 不能使用async，不然不会等待结束
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(IJobExecutionContext context)
        {
            AsyncHelper.RunSync(() => Task.Delay(3000));
            var i = 0;
        }
    }


    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class LongTimeTask : QuartzJobBase
    {
        public override string Name => "模拟耗时任务";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInSeconds(60);

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                Thread.Sleep(1000 * 3 * 10);
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
