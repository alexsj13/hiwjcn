using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ioc;
using Lib.helper;
using Lib.extension;
using Akka;
using Akka.Actor;
using System.Diagnostics;

namespace Hiwjcn.Framework.Actors
{
    public class UpdateEsIndexActor : ReceiveActor
    {
        public UpdateEsIndexActor()
        {
            this.Receive<string>(x =>
            {
                try
                {
                    //
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
