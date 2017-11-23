using Lib.core;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lib.extension;
using Polly.CircuitBreaker;
using System.Configuration;
using Polly;

namespace Lib.data.redis
{
    public static class RedisExtension
    {
        public static IDatabase SelectDatabase(this IConnectionMultiplexer con, int? db)
        {
            if (db == null)
            {
                return con.GetDatabase();
            }
            else
            {
                return con.GetDatabase(db.Value);
            }
        }
    }
}
