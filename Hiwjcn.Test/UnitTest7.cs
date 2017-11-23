using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.extension;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.data;
using org.apache.zookeeper;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;
using Lib.distributed;
using Lib.distributed.zookeeper;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest7 : Watcher
    {
        [TestMethod]
        public async Task fasfda()
        {
            try
            {
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new AlwaysOnZooKeeperClient("localhost:2181");
                client.OnRecconected += () =>
                {
                    "xxx".AddBusinessInfoLog();
                };

                foreach (var i in Com.Range(100))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                    catch (Exception err)
                    {
                        err.AddErrorLog();
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(10));
                await Task.FromResult(1);
            }
            catch (Exception e)
            {
                //
            }
        }

        [TestMethod]
        public async Task khkjhfldskfjkasjdhflgfksadjfaslkj()
        {
            try
            {
                using (var client = new ZooKeeperClient("lib_zookeeper"))
                {
                    await client.Client.DeleteNodeRecursively_("/home");
                }
            }
            catch (Exception e)
            {
                //
            }
        }










        private bool connected = false;
        public override async Task process(WatchedEvent @event)
        {
            if (@event.getState() == Event.KeeperState.SyncConnected)
            {
                this.connected = true;
            }
            if (@event.getState() == Event.KeeperState.Disconnected)
            {
                this.connected = false;
            }
            if (@event.getState() == Event.KeeperState.Expired)
            {
                this.connected = false;
            }
            await Task.FromResult(1);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            //原来zk客户端不应该作为静态对象，每次new

            foreach (var i in Com.Range(100))
            {
                var client = new ZooKeeper("localhost:32771",
                    (int)TimeSpan.FromSeconds(5).TotalMilliseconds, this,
                    canBeReadOnly: false);
                try
                {
                    //var count = 0;
                    //while (client.getState() != ZooKeeper.States.CONNECTED)
                    //{
                    //    if (++count > 100) { throw new Exception("loss patient to wait for connection"); }
                    //    await Task.Delay(10);
                    //}

                    if (await client.existsAsync("/home", false) == null)
                    {
                        var path = await client.createAsync("/home", "".GetBytes(),
                            Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                    }

                    var bs = new { id = 2, name = "fas", age = 44, time = DateTime.Now }.ToJson().GetBytes();

                    await client.setDataAsync("/home", bs);

                    var data = await client.getDataAsync("/home", false);

                    var children = await client.getChildrenAsync("/home");

                    var t = client.transaction();

                    //t.delete("/home");
                    t.setData("/home", $"{DateTime.Now.Ticks}".GetBytes());

                    var res = await t.commitAsync();
                }
                catch (Exception e)
                {
                    //
                }
                finally
                {
                    await client.closeAsync();
                    this.connected = false;
                }
            }
        }
    }
}
