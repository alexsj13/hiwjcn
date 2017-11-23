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
    /// <summary>
    /// redis 老帮助类
    /// 个人觉得对NoSql数据库的操作不要使用异步，因为本身响应就很快，不会阻塞
    /// </summary>
    public static class RedisManager
    {
        /// <summary>
        /// 获取redis连接
        /// </summary>
        /// <param name="handler"></param>
        public static void PrepareConnection(Func<ConnectionMultiplexer, bool> handler) => handler.Invoke(RedisClientManager.Instance.DefaultClient);

        /// <summary>
        /// 获取redis database
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="db_no"></param>
        public static void PrepareDataBase(Func<IDatabase, bool> callback, int? db_no = null)
        {
            PrepareConnection(con =>
            {
                var db = con.SelectDatabase(db_no);

                callback.Invoke(db);

                return true;
            });
        }
    }


    class MQTest
    {
        public MQTest()
        {
            var stop = false;
            RedisManager.PrepareDataBase(db =>
            {
                while (true)
                {
                    if (stop) { break; }

                    var data = db.ListRightPop("es-index");
                    if (!data.HasValue)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    //handler data
                    string json = data;
                    var model = json.JsonToEntity<RedisHelper>();
                }
                return true;
            });
        }
    }

}
