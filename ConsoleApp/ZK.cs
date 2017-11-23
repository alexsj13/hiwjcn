using Lib.distributed.zookeeper;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.distributed.zookeeper.ServiceManager;
using Lib.helper;
using xx;

namespace xx
{
    public interface xxx { }
}

namespace ConsoleApp
{
    [Lib.rpc.ServiceContract_(RelativePath = "jlk.svc")]
    public interface fsadf { }
    [Lib.rpc.ServiceContract_(RelativePath = "jflk.svc")]
    public interface fsfasfadf { }
    [Lib.rpc.ServiceContract_(RelativePath = "jlgk.svc")]
    public interface fsgsdadf { }
    [Lib.rpc.ServiceContract_(RelativePath = "jalk.svc")]
    public interface fsafasdfdf { }

    public class ZK
    {
        public static async Task zk()
        {
            try
            {
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new AlwaysOnZooKeeperClient("es.qipeilong.net:2181");
                client.OnRecconected += () =>
                {
                    Console.WriteLine("重新链接");
                };
                client.OnError += e =>
                {
                    Console.WriteLine(e.GetInnerExceptionAsJson());
                };

                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static async Task sub()
        {
            try
            {
                var host = "es.qipeilong.net:2181";
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new ServiceSubscribe(host);
                client.OnRecconected += () => { Console.WriteLine("重新链接"); };
                client.OnServiceChanged += () =>
                {
                    Console.WriteLine("服务发生更改");
                    client.AllService().Select(x => x.FullName).ToList().ForEach(Console.WriteLine);
                };

                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public class Caller : Lib.rpc.ServiceClient<xxx>
        {
            public Caller(ServiceSubscribe client) : base(client.ResolveSvc<xxx>())
            { }
        }

        public static async Task call()
        {
            try
            {
                var host = "es.qipeilong.net:2181";
                //docker run --name some-zookeeper --restart always -p 2181:2181 -d zookeeper
                var client = new ServiceSubscribe(host);
                client.OnRecconected += () => { Console.WriteLine("重新链接"); };
                client.OnServiceChanged += () =>
                {
                    Console.WriteLine("服务发生更改");
                    client.AllService().Select(x => x.FullName).ToList().ForEach(Console.WriteLine);
                };

                foreach (var i in Com.Range(100))
                {
                    using (var c = new Caller(client))
                    {
                        //
                    }
                }

                await Task.FromResult(1);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
