using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using StackExchange.Redis;
using System.Threading;
using Lib.data.redis;

namespace Lib.distributed.redis
{
    /// <summary>
    /// Redis分布式锁
    /// https://github.com/KidFashion/redlock-cs
    /// </summary>
    public class RedisDistributedLock : IDistributedLock
    {
        public int DB_NUM { get; set; } = 1;
        private string _key { get; set; }
        private byte[] _value { get; set; }
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="retry_count">尝试次数</param>
        /// <param name="retry_delay_ms">尝试延迟，毫秒</param>
        /// <param name="expiry_seconds">锁过期时间，秒</param>
        public RedisDistributedLock(string key, int timeout_ms = 1000 * 10, int expiry_seconds = 30)
        {
            this._key = key;
            this._value = Guid.NewGuid().ToByteArray();
            Retry(timeout_ms, () =>
            {
                var success = false;
                RedisManager.PrepareDataBase(db =>
                {
                    success = db.StringSet(this._key, this._value, expiry: TimeSpan.FromSeconds(expiry_seconds), when: When.NotExists);
                    return true;
                }, DB_NUM);
                return success;
            });
        }
        private void Retry(int timeout_ms, Func<bool> createLock)
        {
            var start = DateTime.Now;
            while (true)
            {
                if (createLock())
                {
                    break;
                }
                if ((DateTime.Now - start).TotalMilliseconds > timeout_ms)
                {
                    throw new Exception($"尝试获取分布式锁失败，超时时间：{timeout_ms}ms");
                }
                Thread.Sleep(200);
            }
        }
        private const string UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";
        public void Dispose()
        {
            RedisManager.PrepareDataBase(db =>
            {
                RedisKey[] key = { this._key };
                RedisValue[] values = { this._value };
                var res = db.ScriptEvaluate(UnlockScript, key, values);
                return true;
            }, DB_NUM);
        }
    }
}
