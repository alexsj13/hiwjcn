using Lib.helper;
using Lib.ioc;
using System;
using Lib.extension;
using System.Threading.Tasks;

namespace Lib.cache
{
    /// <summary>
    /// CacheManager 的摘要说明
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// 如果使用缓存：如果缓存中有，就直接取。如果没有就先获取并加入缓存
        /// 如果不使用缓存：直接从数据源取。
        /// </summary>
        public static T Cache<T>(string key, Func<T> dataSource,
            bool UseCache = true, double expires_minutes = 3)
        {
            //如果读缓存，读到就返回
            if (UseCache)
            {
                return AppContext.Scope(x =>
                {
                    return x.Resolve_<ICacheProvider>().GetOrSet(key, dataSource, TimeSpan.FromMinutes(expires_minutes));
                });
            }
            return dataSource.Invoke();
        }

        /// <summary>
        /// 异步缓存
        /// </summary>
        public static async Task<T> CacheAsync<T>(string key, Func<Task<T>> dataSource,
            bool UseCache = true, double expires_minutes = 3)
        {
            //如果读缓存，读到就返回
            if (UseCache)
            {
                return await AppContext.ScopeAsync(async x =>
                {
                    return await x.Resolve_<ICacheProvider>().GetOrSetAsync(key, dataSource, TimeSpan.FromMinutes(expires_minutes));
                });
            }
            return await dataSource.Invoke();
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveCache(string key)
        {
            AppContext.Scope(x =>
            {
                x.Resolve_<ICacheProvider>().Remove(key);
                return true;
            });
        }

        /// <summary>
        /// 通过正则删除缓存
        /// </summary>
        /// <param name="pattern"></param>
        public static void RemoveByPattern(string pattern)
        {
            AppContext.Scope(x =>
            {
                x.Resolve_<ICacheProvider>().RemoveByPattern(pattern);
                return true;
            });
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void ClearCache()
        {
            AppContext.Scope(x =>
            {
                x.Resolve_<ICacheProvider>().Clear();
                return true;
            });
        }

    }
}