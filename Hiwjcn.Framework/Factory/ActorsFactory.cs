using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Hiwjcn.Framework.Actors;
using Lib.events;
using Lib.extension;
using Lib.core;
using Lib.distributed.akka;

namespace Hiwjcn.Framework.Factory
{
    public static class ActorsFactory
    {
        public static void Dispose()
        {
            try
            {
                ActorsManager<CacheHitLogActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                ActorsManager<ClearCacheActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                ActorsManager<LogRequestActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                ActorsManager<SendMailActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                ActorsManager<UpdateEsIndexActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            try
            {
                ActorsManager<UserActivityLogActor>.Instance.Dispose();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
