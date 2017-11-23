using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    /// <summary>
    /// 使用依赖注入中name为db的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepository<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        public EFManager _EFManager { get; private set; }

        public EFRepository() : this("db") { }
        public EFRepository(string db_name)
        {
            this._EFManager = new EFManager(db_name);
        }

        #region 获取查询上下文

        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            this._EFManager.PrepareIQueryable<T>(callback, track);
        }
        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            await this._EFManager.PrepareIQueryableAsync<T>(callback, track);
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            this._EFManager.PrepareSession(callback);
        }
        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            await this._EFManager.PrepareSessionAsync(callback);
        }

        #endregion
    }

    /// <summary>
    /// 手动传入dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepositoryFromSource<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        private readonly Func<DbContext> GetContext;

        public EFRepositoryFromSource(Func<DbContext> GetContext)
        {
            this.GetContext = GetContext ?? throw new ArgumentNullException(nameof(GetContext));
        }

        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            using (var db = this.GetContext.Invoke())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                callback.Invoke(query);
            }
        }

        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            using (var db = this.GetContext.Invoke())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                await callback.Invoke(query);
            }
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                callback.Invoke(db);
            }
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                await callback.Invoke(db);
            }
        }
    }

    /// <summary>
    /// 通过泛型指定dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public class EFRepositoryFromContext<T, Context> : EFRepositoryBase<T>
        where T : class, IDBTable
        where Context : DbContext, new()
    {
        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            using (var db = new Context())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                callback.Invoke(query);
            }
        }

        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            using (var db = new Context())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                await callback.Invoke(query);
            }
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            using (var db = new Context())
            {
                callback.Invoke(db);
            }
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            using (var db = new Context())
            {
                await callback.Invoke(db);
            }
        }
    }

    /// <summary>
    /// 从依赖注入中获取默认dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepositoryFromIOC<T> : EFRepositoryBase<T>
        where T : class, IDBTable
    {
        private readonly DbContext _context;

        public EFRepositoryFromIOC(DbContext _context)
        {
            this._context = _context;
        }

        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            var query = this._context.Set<T>().AsQueryableTrackingOrNot(track);
            callback.Invoke(query);
        }

        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            var query = this._context.Set<T>().AsQueryableTrackingOrNot(track);
            await callback.Invoke(query);
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            callback.Invoke(this._context);
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            await callback.Invoke(this._context);
        }

        public override void Dispose()
        {
            this._context?.Dispose();
        }
    }
}
