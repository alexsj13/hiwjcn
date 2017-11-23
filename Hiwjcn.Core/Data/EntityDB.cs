#define use_mysql

using Hiwjcn.Core.Model.Sys;
using Lib.core;
using Lib.data;
using Lib.helper;
using Model;
using Model.Category;
using Model.Sys;
using Model.User;
using MySql.Data.Entity;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using WebLogic.Model.Page;
using WebLogic.Model.Sys;
using WebLogic.Model.Tag;
using WebLogic.Model.User;
using Hiwjcn.Core.Domain.Auth;
using System.Data.Entity.SqlServer;
using System.Configuration;
using System.Reflection;
using System;
using Hiwjcn.Core.Domain.WCF;
using Lib.data.ef;

namespace Hiwjcn.Dal
{
    /// <summary>
    /// mysql数据库
    /// </summary>
    public class MySqlConfiguration : DbConfiguration
    {
        public MySqlConfiguration()
        {
            this.SetExecutionStrategy(MySqlProviderInvariantName.ProviderName, () => new MySqlExecutionStrategy());
            this.SetDefaultConnectionFactory(new MySqlConnectionFactory());
        }
    }

    /// <summary>
    /// sqlserver数据库
    /// </summary>
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }

    /// <summary>
    /// EF容器上下文
    /// EF做映射的时候也可以数据库是int，代码里是string，
    /// 用attribute mapping测试成功，用fluent测试失败
    /// </summary>
#if use_mysql_
    [DbConfigurationType(typeof(MySqlConfiguration))]
#else
    [DbConfigurationType(typeof(SqlServerConfiguration))]
#endif
    public class EntityDB : BaseEFContext
    {
        public EntityDB() : base(
            ConfigurationManager.ConnectionStrings["db"]?.ConnectionString ??
            throw new Exception("请配置db数据库链接字符串"))
        {
            //
        }

        #region model对应的实体

        //public virtual DbSet<WcfMap> WcfMap { get; set; }

        public virtual DbSet<AuthClient> AuthClient { get; set; }
        public virtual DbSet<AuthScope> AuthScope { get; set; }
        public virtual DbSet<AuthToken> AuthToken { get; set; }
        public virtual DbSet<AuthTokenScope> AuthTokenScope { get; set; }
        public virtual DbSet<AuthCode> AuthCode { get; set; }

        public virtual DbSet<AuthClientCheckLog> AuthClientCheckLog { get; set; }
        public virtual DbSet<AuthClientUseage> AuthClientUseage { get; set; }

        public virtual DbSet<UserAvatar> UserAvatar { get; set; }
        public virtual DbSet<UserModel> UserModel { get; set; }

        public virtual DbSet<LoginErrorLogModel> LoginErrorLogModel { get; set; }
        public virtual DbSet<RoleModel> RoleModel { get; set; }
        public virtual DbSet<RolePermissionModel> RolePermissionModel { get; set; }
        public virtual DbSet<PermissionModel> PermissionModel { get; set; }
        public virtual DbSet<UserRoleModel> UserRoleModel { get; set; }
        public virtual DbSet<UserOneTimeCode> UserOneTimeCode { get; set; }

        public virtual DbSet<CategoryModel> CategoryModel { get; set; }
        public virtual DbSet<SectionModel> SectionModel { get; set; }
        public virtual DbSet<AreaModel> AreaModel { get; set; }
        public virtual DbSet<CommentModel> CommentModel { get; set; }
        public virtual DbSet<LinkModel> LinkModel { get; set; }
        public virtual DbSet<MessageModel> MessageModel { get; set; }
        public virtual DbSet<OptionModel> OptionModel { get; set; }
        public virtual DbSet<ReqLogModel> ReqLogModel { get; set; }
        public virtual DbSet<CacheHitLog> CacheHitLog { get; set; }
        public virtual DbSet<UpFileModel> UpFileModel { get; set; }
        public virtual DbSet<TagMapModel> TagMapModel { get; set; }
        public virtual DbSet<TagModel> TagModel { get; set; }
        #endregion
    }
}
