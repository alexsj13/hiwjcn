using Elasticsearch.Net;
using Lib.helper;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Lib.core;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.data.elasticsearch
{
    /// <summary>
    /// ES服务器链接管理
    /// </summary>
    public class ElasticsearchClientManager : StaticClientManager<ConnectionSettings>
    {
        public static readonly ElasticsearchClientManager Instance = new ElasticsearchClientManager();

        public override string DefaultKey
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ES"]?.ConnectionString;
            }
        }

        public override bool CheckClient(ConnectionSettings ins)
        {
            return ins != null;
        }

        public override ConnectionSettings CreateNewClient(string key)
        {
            var urls = key.Split('|', ';', ',').Select(s => new Uri(s));
            var pool = new ConnectionSettings(new StaticConnectionPool(urls)).MaximumRetries(2).EnableDebug();
            return pool;
        }

        public override void DisposeClient(ConnectionSettings ins)
        {
            IDisposable dis = ins;
            dis?.Dispose();
        }
    }
}
