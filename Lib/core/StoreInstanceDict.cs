using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.core
{
    /// <summary>
    /// 存储实例
    /// </summary>
    /// <typeparam name="Instance"></typeparam>
    public class StoreInstanceDict<Instance> : Dictionary<string, Instance>
    {
        public readonly object _locker = new object();
    }

    /// <summary>
    /// 扩展
    /// </summary>
    public static class StoreInstanceDictExtension
    {
        /// <summary>
        /// 维护多个实例
        /// </summary>
        public static Instance StoreInstance<Instance>(this StoreInstanceDict<Instance> dict,
            string key, Func<Instance> func,
            Func<Instance, bool> CheckInstance = null, Action<Instance> releaseInstance = null)
        {
            //如果为空就默认都可以
            if (CheckInstance == null) { CheckInstance = _ => _ != null; }

            if (dict.ContainsKey(key))
            {
                var ins = dict[key];
                if (CheckInstance(ins))
                {
                    return ins;
                }
                else
                {
                    releaseInstance?.Invoke(ins);
                    dict.Remove(key);
                }
            }
            if (!dict.ContainsKey(key))
            {
                lock (dict._locker)
                {
                    if (!dict.ContainsKey(key))
                    {
                        var ins = func();
                        if (!CheckInstance(ins)) { throw new Exception("新生成的实例没有通过验证"); }
                        dict[key] = ins;
                    }
                }
            }
            return dict[key];
        }
    }

    /// <summary>
    /// 实例管理
    /// </summary>
    /// <typeparam name="Instance"></typeparam>
    public abstract class StaticClientManager<Instance> : StaticClientManager<Instance, string>
    {
        public override string CreateStringKey(string key)
        {
            return key;
        }
    }

    /// <summary>
    /// T是缓存对象，K是创建T所需要的参数类型，缓存Key通过K去生成
    /// </summary>
    /// <typeparam name="Instance"></typeparam>
    /// <typeparam name="Key"></typeparam>
    public abstract class StaticClientManager<Instance, Key> : IDisposable
    {
        protected readonly StoreInstanceDict<Instance> db = new StoreInstanceDict<Instance>();

        /// <summary>
        /// 创建string key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string CreateStringKey(Key key)
        {
            return key.ToJson().ToSHA1();
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Instance GetCachedClient(Key key)
        {
            var str_key = this.CreateStringKey(key);
            var geter = new Func<Instance>(() => this.CreateNewClient(key));
            var check = new Func<Instance, bool>(this.CheckClient);
            var dispose = new Action<Instance>(this.DisposeClient);

            var ins = this.db.StoreInstance(str_key, geter, check, dispose);
            return ins;
        }

        /// <summary>
        /// 默认客户端
        /// </summary>
        public virtual Instance DefaultClient
        {
            get
            {
                return GetCachedClient(DefaultKey);
            }
        }

        /// <summary>
        /// 释放所有对象,会默认调用DisposeClient
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var kv in this.db)
            {
                DisposeClient(kv.Value);
            }
            this.db.Clear();
        }

        #region 等待实现

        /// <summary>
        /// 默认Key
        /// </summary>
        public abstract Key DefaultKey { get; }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Instance CreateNewClient(Key key);

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="ins"></param>
        public abstract void DisposeClient(Instance ins);

        /// <summary>
        /// 检查对象
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        public abstract bool CheckClient(Instance ins);
        #endregion
    }
}
