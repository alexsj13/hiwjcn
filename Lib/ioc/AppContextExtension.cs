using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Lib.extension;
using System.Data;
using System.Data.Entity;
using Lib.mq;

namespace Lib.ioc
{
    public static class AppContextExtension
    {
        /// <summary>
        /// 使用EF
        /// </summary>
        public static void UseEF<T>(this ContainerBuilder builder, string name = null)
            where T : DbContext
        {
            var context = builder.RegisterType<T>().AsSelf().As<DbContext>();
            if (ValidateHelper.IsPlumpString(name))
            {
                context = context.Named<DbContext>(name);
            }
        }

        /// <summary>
        /// 使用配置
        /// </summary>
        public static void UseSystemConfig<T>(this ContainerBuilder builder)
            where T : class, ISettings =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<ISettings>().SingleInstance();

        /// <summary>
        /// 使用数据库
        /// </summary>
        public static void UseAdoConnection<T>(this ContainerBuilder builder)
            where T : class, IDbConnection =>
            builder.RegisterType<T>().AsSelf().As<IDbConnection>();

        /// <summary>
        /// 使用缓存
        /// </summary>
        public static void UseCacheProvider<T>(this ContainerBuilder builder)
            where T : class, ICacheProvider =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<ICacheProvider>().SingleInstance();

        /// <summary>
        /// 使用消息队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void UseMessageQueue<T>(this ContainerBuilder builder)
            where T : class, IMessageQueueProducer =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<IMessageQueueProducer>().SingleInstance();

        /// <summary>
        /// 获取可以注册的类
        /// </summary>
        public static IEnumerable<Type> FindAllRegistableClass(this Assembly a) =>
            a.GetAllNormalClass().Where(x => x.CanRegIoc());

        /// <summary>
        /// 是否注册为单例
        /// </summary>
        public static bool IsSingleInstance(this Type t) =>
            t.GetCustomAttributes_<SingleInstanceAttribute>().Any();

        /// <summary>
        /// 是否拦截实例
        /// </summary>
        public static bool IsInterceptClass(this Type t) =>
            t.GetCustomAttributes_<InterceptInstanceAttribute>().Any();

        /// <summary>
        /// 配置可以注册IOC，
        /// 要public，不是抽象类，不是接口，没有不注册属性，不是数据表model
        /// </summary>
        public static bool CanRegIoc(this Type t) =>
            t.IsPublic && !t.IsDatabaseTable() && !t.GetCustomAttributes<NotRegIocAttribute>().Any();

        /// <summary>
        /// 给mvc提供依赖注入功能
        /// </summary>
        public static AutofacDependencyResolver AutofacDependencyResolver_(this IContainer context) =>
            new AutofacDependencyResolver(context);

        /// <summary>
        /// 找出所有实例
        /// </summary>
        public static T[] ResolveAll<T>(this ILifetimeScope scope, string name = null)
        {
            return scope.Resolve_<IEnumerable<T>>().ToArray();
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        public static T Resolve_<T>(this ILifetimeScope scope, string name = null)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return scope.ResolveNamed<T>(name);
            }
            else
            {
                return scope.Resolve<T>();
            }
        }

        public static T ResolveKeyed_<T>(this ILifetimeScope scope, object serviceKey) =>
            scope.ResolveKeyed<T>(serviceKey);

        public static T ResolveOptionalKeyed_<T>(this ILifetimeScope scope, object serviceKey)
            where T : class =>
            scope.ResolveOptionalKeyed<T>(serviceKey);

        /// <summary>
        /// 没有注册就返回null
        /// </summary>
        public static T ResolveOptional_<T>(this ILifetimeScope scope, string name = null) where T : class
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return scope.ResolveOptionalNamed<T>(name);
            }
            else
            {
                return scope.ResolveOptional<T>();
            }
        }

        /// <summary>
        /// 不自动dispose对象
        /// </summary>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> DisableAutoDispose<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder) =>
            builder.ExternallyOwned();

        public const string HTTPCONTEXT_AUTOFAC_SCOPE_KEY = "ioc.autofac.scope.key";

        public static void SetAutofacScope(this HttpContext context, ILifetimeScope scope, string context_key = null)
        {
            context.Items[context_key ?? HTTPCONTEXT_AUTOFAC_SCOPE_KEY] = scope ??
                throw new ArgumentNullException(nameof(scope));
        }

        /// <summary>
        /// 获取httpcontext.item中的scope对象，需要配置httpmodule
        /// </summary>
        public static ILifetimeScope GetAutofacScope(this HttpContext context, string context_key = null)
        {
            var obj = context.Items[context_key ?? HTTPCONTEXT_AUTOFAC_SCOPE_KEY];
            if (obj is ILifetimeScope scope)
            {
                return scope;
            }
            throw new Exception("请求中没有缓存autofac scope");
        }

        /// <summary>
        /// 请求上下文的autofac scope
        /// 可以直接拿来resolve
        /// AutofacDependencyResolver.Current.RequestLifetimeScope
        /// </summary>
        public static ILifetimeScope AutofacRequestLifetimeScope(this HttpContext context) =>
            AutofacDependencyResolver.Current.RequestLifetimeScope;

        [Obsolete("改名字了：" + nameof(AutofacRequestLifetimeScope))]
        public static ILifetimeScope MvcAutofacCurrent(this HttpContext context) =>
            context.AutofacRequestLifetimeScope();
    }
}
