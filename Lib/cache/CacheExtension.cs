using Lib.extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.cache
{
    public static class CacheExtension
    {
        public static readonly string CacheKeyPrefix =
            ConfigurationManager.AppSettings["CacheKeyPrefix"] ??
            "Lib.Cache.DefaultKeyPrefix";

        /// <summary>
        /// 缓存key加前缀
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string WithCacheKeyPrefix(this string key) => $"{CacheKeyPrefix}.{key}";

        /// <summary>
        /// 设置数据，并标记为成功
        /// </summary>
        public static void SetSuccessData<T>(this CacheResult<T> data, T model)
        {
            data.Success = true;
            data.Result = model;
        }

        /// <summary>
        /// 缓存逻辑
        /// </summary>
        public static T GetOrSet<T>(this ICacheProvider cacheManager,
            string key, Func<T> dataSource, TimeSpan expire)
        {
            try
            {
                var cache = cacheManager.Get<T>(key);
                if (cache.Success)
                {
                    return cache.Result;
                }
                var data = dataSource.Invoke();
                cacheManager.Set(key, data, expire);
                return data;
            }
            catch (Exception e)
            {
                try
                {
                    //读取缓存失败就尝试移除缓存
                    cacheManager.Remove(key);
                }
                catch (Exception ex)
                {
                    ex.AddErrorLog($"读取缓存异常，删除相关缓存key异常，缓存key:{key}");
                }
                e.AddErrorLog($"读取缓存异常，缓存key:{key}");
                //缓存错误
                return dataSource.Invoke();
            }
        }

        /// <summary>
        /// 异步缓存逻辑
        /// </summary>
        public static async Task<T> GetOrSetAsync<T>(this ICacheProvider cacheManager,
            string key, Func<Task<T>> dataSource, TimeSpan expire)
        {
            try
            {
                var cache = cacheManager.Get<T>(key);
                if (cache.Success)
                {
                    return cache.Result;
                }
                var data = await dataSource.Invoke();
                cacheManager.Set(key, data, expire);
                return data;
            }
            catch (Exception e)
            {
                try
                {
                    //读取缓存失败就尝试移除缓存
                    cacheManager.Remove(key);
                }
                catch (Exception ex)
                {
                    ex.AddErrorLog($"读取缓存异常，删除相关缓存key异常，缓存key:{key}");
                }
                e.AddErrorLog($"读取缓存异常，缓存key:{key}");
                //缓存错误
                return await dataSource.Invoke();
            }
        }
    }
}
