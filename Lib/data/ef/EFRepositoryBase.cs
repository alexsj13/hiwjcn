using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data.ef
{

    public abstract class EFRepositoryBase<T> : IEFRepository<T> where T : class, IDBTable
    {
        #region 添加
        /// <summary>
        /// 添加多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Add(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                count = db.SaveChanges();
                //return true;
            });
            return count;
        }
        /// <summary>
        /// 异步添加
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                count = await db.SaveChangesAsync();
                //return true;
            });
            return count;
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Delete(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                count = db.SaveChanges();
                //return true;
            });
            return count;
        }
        /// <summary>
        /// 异步删除 
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                count = await db.SaveChangesAsync();
                //return true;
            });
            return count;
        }



        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            var count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();

                q = q.WhereIfNotNull(where);

                set.RemoveRange(q);
                count = db.SaveChanges();
                //return true;
            });
            return count;
        }

        public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();

                q = q.WhereIfNotNull(where);

                set.RemoveRange(q);
                count = await db.SaveChangesAsync();
                //return true;
            });
            return count;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 更新多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Update(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                count = db.SaveChanges();
                //return true;
            });
            return count;
        }
        /// <summary>
        /// 异步更新
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                count = await db.SaveChangesAsync();
                //return true;
            });
            return count;
        }

        #endregion

        #region 查询
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetByKeys(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new Exception("参数为空"); }
            T model = default(T);

            PrepareSession(db =>
            {
                model = db.Set<T>().Find(keys);
                //return true;
            });

            return model;
        }

        /// <summary>
        /// 异步查找
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<T> GetByKeysAsync(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new Exception("参数为空"); }
            var model = default(T);

            await PrepareSessionAsync(async db =>
            {
                model = await db.Set<T>().FindAsync(keys);
                //return true;
            });

            return model;
        }

        /// <summary>
        /// 获取list
        /// expression和func的使用注意点，参见lib的readme
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="start">开始位置</param>
        /// <param name="count">读取条数</param>
        /// <param name="Desc">正序反序</param>
        /// <returns></returns>
        public List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            List<T> list = null;
            PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);

                if (orderby != null)
                {
                    if (Desc)
                    {
                        query = query.OrderByDescending(orderby);
                    }
                    else
                    {
                        query = query.OrderBy(orderby);
                    }
                }
                if (start != null)
                {
                    if (orderby == null) { throw new Exception("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                list = query.ToList();
                //return true;
            });
            return ConvertHelper.NotNullList(list);
        }

        public async Task<List<T>> QueryListAsync<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            List<T> list = null;
            await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);

                if (orderby != null)
                {
                    if (Desc)
                    {
                        query = query.OrderByDescending(orderby);
                    }
                    else
                    {
                        query = query.OrderBy(orderby);
                    }
                }
                if (start != null)
                {
                    if (orderby == null) { throw new Exception("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                list = await query.ToListAsync();
                //return true;
            });
            return ConvertHelper.NotNullList(list);
        }

        /// <summary>
        /// 获取list
        /// </summary>
        /// <param name="where"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetList(Expression<Func<T, bool>> where, int? count = null)
        {
            return QueryList<object>(where: where, count: count);
        }
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int? count = null)
        {
            return await QueryListAsync<object>(where: where, count: count);
        }

        /// <summary>
        /// 查询第一个
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public T GetFirst(Expression<Func<T, bool>> where)
        {
            var list = GetList(where: where, count: 1);
            return list.FirstOrDefault();
        }
        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            var list = await GetListAsync(where: where, count: 1);
            return list.FirstOrDefault();
        }

        /// <summary>
        /// 查询记录数（判断记录是否存在请使用Exist方法，效率更高）
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> where)
        {
            int count = 0;
            PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);
                count = query.Count();
                //return true;
            });
            return count;
        }
        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            int count = 0;
            await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);
                count = await query.CountAsync();
                //return true;
            });
            return count;
        }

        /// <summary>
        /// 查询是否存在 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exist(Expression<Func<T, bool>> where)
        {
            bool? ret = null;
            PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);
                ret = query.Any();
                //return true;
            });
            if (ret == null) { throw new Exception("Exist查询失败"); }
            return ret.Value;
        }
        public async Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            bool? ret = null;
            await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);
                ret = await query.AnyAsync();
                //return true;
            });
            if (ret == null) { throw new Exception("Exist查询失败"); }
            return ret.Value;
        }
        #endregion

        #region 获取查询上下文

        public const bool DEFAULT_TRACK = false;

        public abstract void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = DEFAULT_TRACK);

        public abstract Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = DEFAULT_TRACK);

        public abstract void PrepareSession(Func<DbContext, bool> callback);

        public abstract Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback);

        #endregion

        #region 不用返回true的查询上下文

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        public void PrepareIQueryable(Action<IQueryable<T>> callback, bool track = DEFAULT_TRACK) =>
            this.PrepareIQueryable(query => { callback.Invoke(query); return true; }, track: track);

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback, bool track = DEFAULT_TRACK) =>
            await this.PrepareIQueryableAsync(async query => { await callback.Invoke(query); return true; }, track: track);

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        public void PrepareSession(Action<DbContext> callback) =>
            this.PrepareSession(db => { callback.Invoke(db); return true; });

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task PrepareSessionAsync(Func<DbContext, Task> callback) =>
            await this.PrepareSessionAsync(async db => { await callback.Invoke(db); return true; });

        #endregion

        #region 可以直接返回查询结果

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public R PrepareIQueryable_<R>(Func<IQueryable<T>, R> callback, bool track = DEFAULT_TRACK)
        {
            var data = default(R);
            this.PrepareIQueryable(query => { data = callback.Invoke(query); }, track: track);
            return data;
        }

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public async Task<R> PrepareIQueryableAsync_<R>(Func<IQueryable<T>, Task<R>> callback, bool track = DEFAULT_TRACK)
        {
            var data = default(R);
            await this.PrepareIQueryableAsync(async query => { data = await callback.Invoke(query); }, track: track);
            return data;
        }

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <returns></returns>
        public R PrepareSession_<R>(Func<DbContext, R> callback)
        {
            var data = default(R);
            this.PrepareSession(db => { data = callback.Invoke(db); });
            return data;
        }

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<R> PrepareSessionAsync_<R>(Func<DbContext, Task<R>> callback)
        {
            var data = default(R);
            await this.PrepareSessionAsync(async db => { data = await callback.Invoke(db); });
            return data;
        }

        #endregion

        public virtual void Dispose()
        {
            //do nothing
        }

        public List<T> GetListEnsureMaxCount(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            var list = this.GetList(where, count);
            if (list.Count >= count)
            {
                throw new Exception(error_msg);
            }
            return list;
        }

        public async Task<List<T>> GetListEnsureMaxCountAsync(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            var list = await this.GetListAsync(where, count);
            if (list.Count >= count)
            {
                throw new Exception(error_msg);
            }
            return list;
        }
    }
}
