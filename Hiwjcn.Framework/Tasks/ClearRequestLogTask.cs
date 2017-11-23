using Lib.extension;
using Lib.net;
using Lib.ioc;
using Lib.data;
using Lib.task;
using Quartz;
using System;
using System.Threading;
using System.Diagnostics;
using Hiwjcn.Core.Model.Sys;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearRequestLogTask : QuartzJobBase
    {
        public override string Name => "清理请求日志";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInMinutes(5);

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                var expire = DateTime.Now.AddDays(-30);
                AppContext.Scope(s =>
                {
                    s.Resolve_<IRepository<ReqLogModel>>().DeleteWhere(x => x.CreateTime < expire);
                    s.Resolve_<IRepository<CacheHitLog>>().DeleteWhere(x => x.CreateTime < expire);
                    return true;
                });
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
