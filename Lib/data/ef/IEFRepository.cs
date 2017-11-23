using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;

namespace Lib.data.ef
{
    public interface IEFRepository<T> : ILinqRepository<T>, IRepository<T>
        where T : IDBTable
    {
        /// <summary>
        /// 获取session
        /// </summary>
        [Obsolete("返回bool无意义")]
        void PrepareSession(Func<DbContext, bool> callback);

        /// <summary>
        /// 获取session
        /// </summary>
        [Obsolete("返回bool无意义")]
        Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback);

        /// <summary>
        /// 不用return true
        /// </summary>
        void PrepareSession(Action<DbContext> callback);

        /// <summary>
        /// 不用return true
        /// </summary>
        Task PrepareSessionAsync(Func<DbContext, Task> callback);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        R PrepareSession_<R>(Func<DbContext, R> callback);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        Task<R> PrepareSessionAsync_<R>(Func<DbContext, Task<R>> callback);
    }
}
