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
using Lib.data.elasticsearch;

namespace ConsoleApp
{
    public static class ES
    {
        private static readonly string ES_SERVERS = "http://localhost:9200";

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x))));

        /// <summary>
        /// 索引磁盘上的文件
        /// </summary>
        public static void IndexFiles()
        {
            var client = new ElasticClient(setting);
            var indexName = nameof(DiskFileIndex).ToLower();
            try
            {
                client.CreateIndexIfNotExists(indexName, x => x.GetCreateIndexDescriptor<DiskFileIndex>());
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }

            var txtFiles = new string[] {
                    ".txt", ".cs", ".css", ".js", ".cshtml", ".html"
                    , ".readme", ".json", ".config", ".md" };

            var maxSize = Com.MbToB(1);

            Com.FindFiles("D:\\XXXXXXXXXXXXXXXXXXXX\\", (f) =>
            {
                Thread.Sleep(100);
                try
                {
                    if (!txtFiles.Any(x => ConvertHelper.GetString(f.Name).ToLower().EndsWith(x))
                    || ConvertHelper.GetString(f.FullName).ToLower().Contains("elasticsearch")
                    || f.Length > maxSize)
                    {
                        Console.WriteLine($"跳过文件：{f.FullName}");
                        return;
                    }
                    var model = new DiskFileIndex();
                    model.Path = f.FullName;
                    model.Name = f.Name;
                    model.Extension = f.Extension;
                    model.Size = f.Length;
                    model.Content = File.ReadAllText(model.Path);
                    var data = new List<DiskFileIndex>() { model };
                    client.AddToIndex(indexName, data.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }, (c) =>
            {
                Console.WriteLine($"剩余任务：{c}");
                if (c > 5000) { Thread.Sleep(400); }
            });
            Console.ReadLine();
        }

        [ElasticsearchType(IdProperty = "Path", Name = "diskfileindex")]
        public class DiskFileIndex : IElasticSearchIndex
        {
            [String(Name = "Path", Index = FieldIndexOption.NotAnalyzed)]
            public virtual string Path { get; set; }

            [Number(Name = "Size", Index = NonStringIndexOption.NotAnalyzed)]
            public virtual long Size { get; set; }

            [String(Name = "Name", Index = FieldIndexOption.NotAnalyzed)]
            public virtual string Name { get; set; }

            [String(Name = "Extension", Index = FieldIndexOption.NotAnalyzed)]
            public virtual string Extension { get; set; }

            [String(Name = "Content", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public virtual string Content { get; set; }
        }
    }
}
