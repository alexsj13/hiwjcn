using Autofac;
using Hiwjcn.Bll;
using Hiwjcn.Dal;
using Lib.cache;
using Lib.ioc;
using Lib.core;
using Lib.mvc.user;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Entity;
using WebCore.MvcLib.Controller;
using Lib.helper;
using System;
using Bll.User;
using Hiwjcn.Bll.Auth;
using Lib.mvc.auth;
using Lib.data;
using Lib.infrastructure;
using Lib.data.ef;

namespace Hiwjcn.Framework
{
    public class CommonDependencyRegister : DependencyRegistrarBase
    {
        public override void Register(ref ContainerBuilder builder)
        {
            var tps = new
            {
                framework = typeof(UserBaseController).Assembly,
                service = typeof(MyService).Assembly,
                core = typeof(EntityDB).Assembly
            };

            //Aop拦截
            builder.RegisterType<AopLogError_>();
            //缓存
            if (ValidateHelper.IsPlumpString(ConfigHelper.Instance.RedisConnectionString))
            {
                builder.UseCacheProvider<RedisCacheProvider>();
            }
            else
            {
                builder.UseCacheProvider<MemoryCacheProvider>();
            }
            builder.UseSystemConfig<BasicConfigProvider>();
            builder.UseEF<EntityDB>("db");
            builder.UseAdoConnection<MySqlConnection>();
            //builder.RegisterInstance(new LoginStatus()).As<LoginStatus>().SingleInstance();
            //builder.Register(_ => new LoginStatus("hiwjcn_uid", "hiwjcn_token", "hiwjcn_login_session", "")).AsSelf().As<ILoginStatus>().SingleInstance();

            #region 自动注册
            AutoRegistered(ref builder, tps.core, tps.service, tps.framework);
            #endregion

            #region 注册Data
            //注册数据访问层
            RegDataRepository_(ref builder, tps.core);
            RegEFDataRepositoryProvider(ref builder, typeof(EFRepository<>));
            #endregion

            #region 注册service
            //逻辑代码注册
            RegService_(ref builder, tps.service);
            RegServiceProvider(ref builder, typeof(ServiceBase<>));
            #endregion

            #region 注册事件
            //事件注册
            RegEvent(ref builder, tps.service);
            #endregion
        }
    }
}
