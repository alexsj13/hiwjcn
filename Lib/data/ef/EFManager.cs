using Lib.extension;
using Lib.helper;
using Lib.ioc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Autofac;

namespace Lib.data.ef
{
    /// <summary>
    /// EF的帮助类
    /// </summary>
    public partial class EFManager
    {
        private readonly string db_name;
        public EFManager(string name)
        {
            this.db_name = name;
        }

        /// <summary>
        /// 获取错误
        /// </summary>
        /// <returns></returns>
        public string GetValidationErrors()
        {
            var errors = string.Empty;
            PrepareSession(db =>
            {
                errors = db.GetValidationErrors().ToJson();
                return true;
            });
            return errors;
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext(ILifetimeScope scope, string db = null)
        {
            /* 提升性能，不要检查
            if (!AppContext.IsRegistered<DbContext>(db_name))
            {
                throw new Exception("请在容器中注册dbcontext实例");
            }*/
            return scope.Resolve_<DbContext>(name: db ?? db_name);
        }

        /// <summary>
        /// 准备实体容器对象
        /// </summary>
        /// <param name="callback"></param>
        public void PrepareSession(Func<DbContext, bool> callback)
        {
            AppContext.Scope(x =>
            {
                using (var db = GetDbContext(x))
                {
                    //执行回调
                    if (!callback.Invoke(db))
                    {
                        //执行失败
                    }
                }
                return true;
            });
        }

        public async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            await AppContext.ScopeAsync(async x =>
            {
                using (var db = GetDbContext(x))
                {
                    //执行回调
                    if (!(await callback.Invoke(db)))
                    {
                        //执行失败
                    }
                }
                return true;
            });
        }

        /// <summary>
        /// 准备IQueryable用于linq查询
        /// Where方法里不是func而是expression，
        /// 这个不会用来执行只会用来分析表达式树，不会出现空指针现象（x.age==18）
        /// </summary>
        public void PrepareIQueryable<T>(Func<IQueryable<T>, bool> callback, bool track = true)
            where T : class, IDBTable
        {
            PrepareSession(session =>
            {
                //用来查询，所以不要跟踪实体状态
                //NopCommerce:
                //Gets a table with "no tracking" enabled (EF feature) 
                //Use it only when you load record(s) only for read-only operations
                var query = session.Set<T>().AsQueryableTrackingOrNot(track);
                return callback.Invoke(query);
            });
        }

        public async Task PrepareIQueryableAsync<T>(Func<IQueryable<T>, Task<bool>> callback, bool track = true)
            where T : class, IDBTable
        {
            await PrepareSessionAsync(async session =>
            {
                //用来查询，所以不要跟踪实体状态
                //NopCommerce:
                //Gets a table with "no tracking" enabled (EF feature) 
                //Use it only when you load record(s) only for read-only operations
                var query = session.Set<T>().AsQueryableTrackingOrNot(track);
                return await callback.Invoke(query);
            });
        }

    }

    partial class EFManager
    {
        /// <summary>
        /// 取消EF首次访问数据库的System.Data.Entity.CreateDatabaseIfNotExists策略
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        public static void SetNoInitializer<T>(T db) where T : DbContext, new()
        {
            SetNoInitializer<T>();
        }
        public static void SetNoInitializer<T>() where T : DbContext, new()
        {
            Database.SetInitializer<T>(new NullDatabaseInitializer<T>());
        }

        public static void FastStart<T>() where T : DbContext, new()
        {
            SetNoInitializer<T>();
            using (var db = new T())
            {
                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
            }
        }

        /// <summary>
        /// 尝试创建数据表
        /// </summary>
        public static void TryInstallDatabase<T>() where T : DbContext, new()
        {
            using (var db = new T())
            {
                var switcher = false;
                if (switcher)
                {
                    db.CreateDatabaseIfNotExist();
                }
                else
                {
                    db.TryCreateTable();
                }
            }
        }
    }
}
