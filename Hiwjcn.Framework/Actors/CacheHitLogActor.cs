using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Hiwjcn.Core.Model.Sys;
using Lib.data;
using Lib.helper;
using Lib.extension;
using Lib.core;
using Lib.ioc;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 记录缓存命中的情况
    /// </summary>
    public class CacheHitLogActor : ReceiveActor
    {
        public CacheHitLogActor()
        {
            this.Receive<CacheHitLog>(x =>
            {
                try
                {
                    x.Init("cachehitlog");

                    $"缓存命中情况：{x.ToJson()}".AddBusinessInfoLog();

                    AppContext.Scope(s =>
                    {
                        s.Resolve_<IRepository<CacheHitLog>>().Add(x);
                        return true;
                    });
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
