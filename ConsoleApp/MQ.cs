using Lib.extension;
using Lib.mq;
using Lib.mq.rabbitmq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public static class MQ
    {
        public static void Consumer()
        {
            var _factory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                UseBackgroundThreadsForIO = true,
                HostName = "mq.qipeilong.net",
                UserName = "admin",
                Password = "mypass",
                VirtualHost = "/",
                NetworkRecoveryInterval = TimeSpan.FromSeconds(1)
            };
            using (var con = _factory.CreateConnection())
            {
                using (var consumer = new Worker(con.CreateModel()))
                {
                    Console.ReadLine();
                }
            }
        }

        public class Worker : RabbitMqConsumerBase
        {
            public Worker(IModel channel) :
                base(channel, "测试消费", "test_exchange", "test_queue", "#.msg.#",
                    ExchangeTypeEnum.topic, true, true, 10, true)
            { }

            public override async Task<bool?> OnMessageReceived(object sender, BasicDeliverEventArgs args)
            {
                try
                {
                    var msg = Encoding.UTF8.GetString(args.Body);
                    Console.WriteLine(msg);
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
                return await Task.FromResult(true);
            }
        }
    }
}
