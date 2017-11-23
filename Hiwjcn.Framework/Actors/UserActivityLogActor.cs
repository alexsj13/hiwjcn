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
    public class UserActivityLogActor : ReceiveActor
    {
        public UserActivityLogActor()
        {
            this.Receive<UserActivity>(x =>
            {
                try
                {
                    x.Init("ua");
                    if (!x.IsValid(out var msg))
                    {
                        msg.AddBusinessInfoLog();
                        return;
                    }
                    AppContext.Scope(s =>
                    {
                        s.Resolve_<IRepository<UserActivity>>().Add(x);
                        return true;
                    });
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            });
        }
    }
}
