using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.data.redis;

namespace Lib.cache
{
    /// <summary>
    /// 目前只是思路
    /// </summary>
    public class FireWall
    {
        private RedisHelper redis;
        private TimeSpan expire;
        private double limit;

        public FireWall() : this(TimeSpan.FromMinutes(1), 30)
        { }

        public FireWall(TimeSpan expire, double limit)
        {
            this.redis = new RedisHelper();
            this.expire = expire;
            this.limit = limit;
            if (this.limit <= 0) { throw new Exception("limit不能小于1"); }
        }

        public bool Hit(string key)
        {
            //var first = !this.redis.KeyExists(key);
            var count = this.redis.StringIncrement(key, 1);
            var first = count == 1;
            if (first)
            {
                if (!this.redis.KeyExpire(key, TimeSpan.FromMinutes(1))) { throw new Exception("无法设置key过期"); }
            }

            return count <= this.limit;
        }

        public double HitCount(string key)
        {
            var count = this.redis.StringGet<double>(key);

            return count;
        }

    }
}
