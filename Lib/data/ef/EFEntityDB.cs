using Lib.helper;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using System.Reflection;

namespace Lib.data.ef
{

    /// <summary>
    /// EF
    /// </summary>
    public abstract class BaseEFContext : DbContext
    {
        public BaseEFContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;

            this.Database.CommandTimeout = 3;
            this.Database.Log = (log) =>
            {
                //if (!ValidateHelper.IsPlumpString(log)) { return; }
                //LogHelper.Error(typeof(EntityDB), "EF日志：\n" + log);
            };
        }

        /// <summary>
        /// 注册mapping
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //不在表名后加s或者es
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //注册映射
            var ass = this.FindRegisterTableFluentMappingAssembly();
            if (ValidateHelper.IsPlumpList(ass))
            {
                foreach (var a in ass)
                {
                    //手动注册
                    //modelBuilder.Configurations.Add(new UserModelMapping());
                    //通过反射自动注册
                    modelBuilder.RegisterTableFluentMapping(a);
                }
            }
        }

        protected virtual Assembly[] FindRegisterTableFluentMappingAssembly()
        {
            return new Assembly[] { };
        }

        /// <summary>
        /// Attach an entity to the context or return an already attached entity (if it was already attached)
        /// 附加一个实体，如果已经存在就直接返回
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Attached entity</returns>
        //protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : ModelBase, new()
        //{
        //    //little hack here until Entity Framework really supports stored procedures
        //    //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
        //    var alreadyAttached = Set<TEntity>().Local.FirstOrDefault(x => x.Id == entity.Id);
        //    if (alreadyAttached == null)
        //    {
        //        //attach new entity
        //        Set<TEntity>().Attach(entity);
        //        return entity;
        //    }

        //    //entity is already loaded
        //    return alreadyAttached;
        //}

        /// <summary>
        /// 实体集合
        /// new的用法搜索 override new
        /// </summary>
        public new IDbSet<T> Set<T>() where T : class, IDBTable
        {
            return base.Set<T>();
        }

    }

    /// <summary>
    /// EF mapping的方式
    /// </summary>
    public enum EFConfigEnum : int
    {
        Attribute = 1,
        Fluent = 2
    }

    /// <summary>
    /// EF配置
    /// </summary>
    public interface IEFConfig
    {
        string ConnectionString { get; }

        bool LazyLoad { get; }

        bool ValidModelWhenSave { get; }

        int CommandTimeoutSeconds { get; }

        EFConfigEnum ConfigType { get; }

        Assembly[] EntityAssemblies { get; }

        void AddLog(string log);
    }

    /// <summary>
    /// 备注一下：通过这种方式可以配置DefaultConnectionFactory
    /// 通过DbConfigurationType标签可以添加到EFDBCONTEXT上
    /// https://msdn.microsoft.com/en-us/data/jj680699
    /// </summary>
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("v11.0"));
        }
    }
    public class MySqlConfiguration : DbConfiguration
    {
        public MySqlConfiguration()
        {
            //SetDefaultConnectionFactory(new MySql.Data.Entity.MySqlConnectionFactory());
        }
    }


    /// <summary>
    /// 通过ioc创建实例
    /// </summary>
    public class EFEntityDB : BaseEFContext
    {
        private readonly IEFConfig _config;

        public EFEntityDB(IEFConfig config) : base(config.ConnectionString)
        {
            this._config = config;

            this.Configuration.LazyLoadingEnabled = _config.LazyLoad;
            this.Configuration.ValidateOnSaveEnabled = _config.ValidModelWhenSave;

            this.Database.CommandTimeout = _config.CommandTimeoutSeconds;

            this.Database.Log = new Action<string>(_config.AddLog);
        }

        /// <summary>
        /// 注册mapping
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //不在表名后加s或者es
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            var ass = this._config.EntityAssemblies;
            if (!ValidateHelper.IsPlumpList(ass))
            {
                throw new Exception("EF:实体程序集不允许为空");
            }

            var mappingType = this._config.ConfigType;
            if (mappingType == EFConfigEnum.Attribute)
            {
                modelBuilder.RegisterTableAttributeMapping(ass);
            }
            else if (mappingType == EFConfigEnum.Fluent)
            {
                modelBuilder.RegisterTableFluentMapping(ass);
            }
            else
            {
                throw new Exception("EF:不支持的mapping方式");
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
