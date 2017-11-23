using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using RazorEngine;
using RazorEngine.Templating;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.distributed.akka;
using Akka.Actor;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest10
    {
        class SubWorker : KillableReceiveActor
        {
            public SubWorker()
            {
                //
            }

            protected override void OnReceivePoisonPill(ActorPoisonPill pill)
            {
                base.OnReceivePoisonPill(pill);
            }
        }

        class Worker : KillableReceiveActor
        {
            private readonly IActorRef _sub = Context.ActorOf<SubWorker>();

            public Worker() : base()
            {
                this.Receive<int>(x =>
                {
                    if (x == 3)
                    {
                        Context.Stop(Self);
                    }
                });
            }
        }

        [TestMethod]
        public void fasdfasdflkjalh()
        {
            try
            {
                using (var sys = AkkaSystemManager.Instance.DefaultClient)
                {
                    var actor = sys.CreateActor<Worker>("wj");

                    actor.Tell(1);
                    actor.Tell(30);
                    actor.Tell(2);

                    actor.FeedPoisonPill();
                }
            }
            catch (Exception e)
            {
                //
            }
        }

        public class User
        {
            public virtual string Name { get; set; }
            public virtual int Age { get; set; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var logger = new Action<long, string>((ms, name) =>
            {
                $"{name}耗时{ms}毫秒".DebugInfo();
            });
            var user = new User() { Name = "wj", Age = 25 };
            var count = 10000;
            //string template
            using (var timer = new CpuTimeLogger(logger, "string template"))
            {
                foreach (var i in Com.Range(count))
                {
                    var temp = new Antlr4.StringTemplate.Template("用户：<user.Name>，年龄：<user.Age>");
                    temp.Add("user", user);
                    var data = temp.Render();
                }
            }
            //razor
            using (var timer = new CpuTimeLogger(logger, "razor engine"))
            {
                Engine.Razor.RunCompile(
                    templateSource: "用户：@Model.Name，年龄：@Model.Age",
                    name: "a",
                    modelType: user.GetType(),
                    model: user);
                foreach (var i in Com.Range(count))
                {
                    var data = Engine.Razor.Run("a", null, user);
                }
            }
        }
    }
}
