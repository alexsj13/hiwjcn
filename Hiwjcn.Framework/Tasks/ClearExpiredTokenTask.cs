using Lib.extension;
using Lib.net;
using Lib.task;
using Quartz;
using System;
using System.Threading;
using Lib.core;
using Lib.ioc;
using Lib.data;
using System.Linq;
using Hiwjcn.Core.Infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.infrastructure.entity;
using Lib.data.ef;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class ClearExpiredTokenTask : QuartzJobBase
    {
        public override string Name => "清理过期的Token";

        public override bool AutoStart => true;

        public override ITrigger Trigger => this.TriggerIntervalInMinutes(5);

        public override void Execute(IJobExecutionContext context)
        {
            try
            {
                AppContext.Scope(s =>
                {
                    var repo = s.Resolve_<IEFRepository<AuthToken>>();
                    repo.PrepareSession(db =>
                    {
                        var now = DateTime.Now;
                        var token_set = db.Set<AuthToken>();
                        var code_set = db.Set<AuthCode>();
                        var scope_set = db.Set<AuthScope>();
                        var scope_map_set = db.Set<AuthTokenScope>();

                        //delete tokens
                        token_set.RemoveRange(token_set.Where(x => x.ExpiryTime < now));
                        db.SaveChanges();

                        //delete codes
                        var expire = now.AddMinutes(-TokenConfig.CodeExpireMinutes);
                        code_set.RemoveRange(code_set.Where(x => x.CreateTime < expire));
                        db.SaveChanges();

                        //delete token_scope
                        var token_uids = token_set.Select(x => x.UID);
                        var scope_uids = scope_set.Select(x => x.UID);
                        scope_map_set.RemoveRange(scope_map_set.Where(x => !token_uids.Contains(x.TokenUID) || !scope_uids.Contains(x.ScopeUID)));
                        db.SaveChanges();

                        return true;
                    });
                    return true;
                });
                "成功清理过期token，以及相关信息".AddBusinessInfoLog();
            }
            catch (Exception e)
            {
                e.AddErrorLog(this.Name);
            }
        }
    }
}
