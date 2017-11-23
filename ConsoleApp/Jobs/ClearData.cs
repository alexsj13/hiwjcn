using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.task;
using Quartz;

namespace ConsoleApp.Jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearData : QuartzJobBase
    {
        public override string Name => "test";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInSeconds(30);

        public override void Execute(IJobExecutionContext context)
        {
            //
        }
    }
}
