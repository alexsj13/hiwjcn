using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Lib.helper;
using Lib.extension;
using Lib.log;

namespace Lib.cache
{
    /// <summary>
    /// 请求周期内缓存
    /// </summary>
    public class HttpContextCacheProvider : CacheBase, ICacheProvider
    {
        protected IDictionary Cache
        {
            get => HttpContext.Current?.Items ??
                throw new Exception($"{nameof(HttpContextCacheProvider)}只能在web环境中使用");
        }

        public void Clear()
        {
            this.Cache.Clear();
        }

        public void Dispose()
        {
            //do nothing
        }

        public CacheResult<T> Get<T>(string key)
        {
            var data = this.Cache[key];
            if (data is byte[] bs)
            {
                var res = this.Deserialize<CacheResult<T>>(bs);
                if (res != null)
                {
                    res.Success = true;
                    return res;
                }
            }
            return new CacheResult<T>() { Success = false };
        }

        public bool IsSet(string key)
        {
            return this.Cache.Contains(key);
        }

        public void Remove(string key)
        {
            this.Cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();

            foreach (string key in Cache.Keys)
            {
                if (regex.IsMatch(key))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (string key in keysToRemove)
            {
                Remove(key);
            }
        }

        public void Set(string key, object data, TimeSpan expire)
        {
            if (expire.TotalMilliseconds > 0)
            {
                $"{nameof(HttpContextCacheProvider)}不支持缓存过期".DebugInfo();
            }

            var res = new CacheResult<object>() { Result = data, Success = true };
            this.Cache[key] = this.Serialize(res);
        }
    }
}
