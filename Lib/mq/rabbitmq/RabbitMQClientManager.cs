using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.mq.rabbitmq
{
    public class RabbitMQClientManager : StaticClientManager<RabbitMqClient>
    {
        public static readonly RabbitMQClientManager Instance = new RabbitMQClientManager();

        public override string DefaultKey
        {
            get
            {
                return "lib_rabbitmq";
            }
        }

        public override bool CheckClient(RabbitMqClient ins)
        {
            return ins != null && ins.Connection.IsOpen;
        }

        public override RabbitMqClient CreateNewClient(string key)
        {
            return new RabbitMqClient(key);
        }

        public override void DisposeClient(RabbitMqClient ins)
        {
            ins?.Dispose();
        }
    }
}
