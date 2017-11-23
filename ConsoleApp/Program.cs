using Elasticsearch.Net;
using Fleck;
using Lib.core;
using Lib.distributed;
using Lib.distributed.zookeeper;
using Lib.extension;
using Lib.helper;
using Lib.io;
using Lib.mq;
using Lib.mq.rabbitmq;
using Lib.rpc;
using Nest;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using System.Text;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //ES.IndexFiles();

            //FleckWS.ws();

            //MQ.Consumer();

            //ZK.zk().Wait();
            ZK.sub().Wait();

            //JobManager.Start();

            Console.WriteLine("finish");
            Console.ReadLine();
            Lib.core.LibReleaseHelper.DisposeAll();
        }
    }


}