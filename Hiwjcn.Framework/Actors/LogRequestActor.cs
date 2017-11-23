using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Lib.core;
using Lib.helper;
using Lib.extension;
using System.Threading;
using Hiwjcn.Core.Model.Sys;
using System.Diagnostics;
using Lib.ioc;
using Lib.data;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 异步记录请求记录
    /// </summary>
    public class LogRequestActor : ReceiveActor
    {
        public LogRequestActor()
        {
            this.Receive<ReqLogModel>(x =>
            {
                try
                {
                    x.Init("reqlog");
                    AppContext.Scope(s =>
                    {
                        return s.Resolve_<IRepository<ReqLogModel>>().Add(x);
                    });
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }

    public class TestActor : ReceiveActor
    {
        public TestActor()
        {
            this.Receive<string>(x =>
            {
                Thread.Sleep(500);
                x?.AddBusinessInfoLog();

                //这里stop似乎就可以释放资源
                //测试是这样的，还有待求证
                Context.Stop(this.Self);
            });
        }
    }
}
