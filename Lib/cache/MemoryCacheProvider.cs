using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Lib.cache
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    public class MemoryCacheProvider : CacheBase, ICacheProvider
    {
        /// <summary>
        /// Cache object
        /// </summary>
        protected ObjectCache Cache
        {
            get => MemoryCache.Default ?? throw new Exception("无法使用内存缓存");
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual CacheResult<T> Get<T>(string key)
        {
            var data = Cache[key];
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

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        public virtual void Set(string key, object data, TimeSpan expire)
        {
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + expire;

            var res = new CacheResult<object>() { Result = data, Success = true };
            Cache.Add(new CacheItem(key, this.Serialize(res)), policy);
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public virtual bool IsSet(string key)
        {
            return Cache.Contains(key);
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public virtual void Remove(string key)
        {
            Cache.Remove(key);
        }

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public virtual void RemoveByPattern(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();

            foreach (var item in Cache)
            {
                if (regex.IsMatch(item.Key))
                {
                    keysToRemove.Add(item.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            foreach (var item in Cache)
            {
                Remove(item.Key);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            //do nothing
        }
    }
}