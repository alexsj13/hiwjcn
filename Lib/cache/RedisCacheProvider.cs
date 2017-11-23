using System;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using Lib.core;
using Lib.helper;
using Lib.data;
using System.Configuration;
using Lib.data.redis;

namespace Lib.cache
{
    /// <summary>
    /// Represents a manager for caching in Redis store (http://redis.io/).
    /// Mostly it'll be used when running in a web farm or Azure.
    /// But of course it can be also used on any server or environment
    /// </summary>
    public class RedisCacheProvider : CacheBase, ICacheProvider
    {
        private readonly int CACHE_DB =
            Lib.helper.ConvertHelper.GetInt(ConfigurationManager.AppSettings["RedisCacheDB"], 1);

        #region Methods

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual CacheResult<T> Get<T>(string key)
        {
            var cache = new CacheResult<T>() { Success = false };
            RedisManager.PrepareDataBase(_db =>
            {
                var rValue = _db.StringGet(key);
                if (rValue.HasValue)
                {
                    var res = this.Deserialize<CacheResult<T>>(rValue);
                    if (res != null)
                    {
                        res.Success = true;
                        cache = res;
                    }
                }
                return true;
            }, CACHE_DB);
            return cache;
        }

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        public virtual void Set(string key, object data, TimeSpan expire)
        {
            RedisManager.PrepareDataBase(_db =>
            {
                var res = new CacheResult<object>() { Success = true, Result = data };
                var entryBytes = this.Serialize(res);

                _db.StringSet(key, entryBytes, expire);
                return true;
            }, CACHE_DB);
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public virtual bool IsSet(string key)
        {
            bool ret = false;
            RedisManager.PrepareDataBase(_db =>
            {
                ret = _db.KeyExists(key);
                return true;
            }, CACHE_DB);
            return ret;
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public virtual void Remove(string key)
        {
            RedisManager.PrepareDataBase(_db =>
            {
                _db.KeyDelete(key);
                return true;
            }, CACHE_DB);
        }

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public virtual void RemoveByPattern(string pattern)
        {
            RedisManager.PrepareConnection(_muxer =>
            {
                var _db = _muxer.GetDatabase(CACHE_DB);
                foreach (var ep in _muxer.GetEndPoints())
                {
                    var server = _muxer.GetServer(ep);
                    var keys = server.Keys(pattern: "*" + pattern + "*");
                    foreach (var key in keys)
                        _db.KeyDelete(key);
                }
                return true;
            });
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            RedisManager.PrepareConnection(_muxer =>
            {
                var _db = _muxer.GetDatabase(CACHE_DB);
                foreach (var ep in _muxer.GetEndPoints())
                {
                    var server = _muxer.GetServer(ep);
                    //we can use the code belwo (commented)
                    //but it requires administration permission - ",allowAdmin=true"
                    //server.FlushDatabase();

                    //that's why we simply interate through all elements now
                    var keys = server.Keys();
                    foreach (var key in keys)
                        _db.KeyDelete(key);
                }
                return true;
            });
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            //do nothing
        }

        #endregion
    }
}