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
    /// <summary>
    /// IOC容器
    /// https://autofac.org/
    /// http://autofac.readthedocs.io/en/latest/getting-started/index.html
    /// </summary>
    public static class AppContext
    {
        /// <summary>
        /// 再更高层添加数据
        /// </summary>
        private static readonly List<IDependencyRegistrar> ExtraRegistrars = new List<IDependencyRegistrar>();

        /// <summary>
        /// 添加额外的注册（这个操作要尽量早执行）
        /// </summary>
        /// <param name="reg"></param>
        public static void AddExtraRegistrar(IDependencyRegistrar reg)
        {
            if (context != null)
            {
                throw new Exception("依赖注入容器已经生成，请在生成前注册额外依赖");
            }
            ExtraRegistrars.Add(reg);
        }

        /// <summary>
        /// 创建容器线程锁
        /// </summary>
        private static readonly object LOCKER = new object();

        /// <summary>
        /// 容器对象
        /// </summary>
        private static IContainer context = null;

        /// <summary>
        /// 销毁容器
        /// </summary>
        public static void Dispose()
        {
            context?.Dispose();
        }

        public static RefAction<ContainerBuilder> OnContainerBuilding { get; set; }

        /// <summary>
        /// 获取ioc容器，第一次访问将创建容器
        /// </summary>
        /// <returns></returns>
        public static IContainer Container
        {
            get
            {
                if (context == null)
                {
                    lock (LOCKER)
                    {
                        if (context == null)
                        {
                            //创建builder
                            var builder = new ContainerBuilder();
                            //注册依赖
                            new BaseDependencyRegistrar().Register(ref builder);
                            //注册额外依赖
                            if (ValidateHelper.IsPlumpList(ExtraRegistrars))
                            {
                                foreach (var reg in ExtraRegistrars)
                                {
                                    reg.Register(ref builder);
                                    reg.Clean();
                                }
                            }

                            //额外的切入点
                            OnContainerBuilding?.Invoke(ref builder);

                            //创建容器
                            context = builder.Build();
                        }
                    }
                }
                return context;
            }
        }

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsRegistered<T>() => Container.IsRegistered<T>();

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegisteredWithName<T>(string name) => Container.IsRegisteredWithName<T>(name);

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegistered<T>(string name)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return IsRegisteredWithName<T>(name);
            }
            else
            {
                return IsRegistered<T>();
            }
        }
        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete("不使用生命周期直接使用autofac创建实例，实例会被autofac跟踪，不会被释放")]
        public static T GetObject<T>(string name = null)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return Container.ResolveNamed<T>(name);
            }
            else
            {
                return Container.Resolve<T>();
            }
        }
        /// <summary>
        /// 获取所有实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete("不使用生命周期直接使用autofac创建实例，实例会被autofac跟踪，不会被释放")]
        public static T[] GetAllObject<T>(string name = null)
        {
            return GetObject<IEnumerable<T>>(name).ToArray();
        }

        /// <summary>
        /// 创建一个作用域
        /// </summary>
        /// <returns></returns>
        public static ILifetimeScope Scope() => Container.BeginLifetimeScope();

        /// <summary>
        /// 生命周期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Scope<T>(Func<ILifetimeScope, T> func)
        {
            var catch_exception_and_create_scope = true;
            try
            {
                //尝试使用httpscope
                var context = HttpContext.Current;
                if (context != null)
                {
                    var s = context.GetAutofacScope();
                    //已经成功创建scope，没有必要继续尝试创建
                    catch_exception_and_create_scope = false;
                    return func.Invoke(s);
                }
            }
            catch when (catch_exception_and_create_scope)
            {
                //do nothing
            }

            //httpcontext中没有scope，创建一次性scope
            using (var scope = Scope())
            {
                return func.Invoke(scope);
            }
        }

        /// <summary>
        /// 生命周期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<T> ScopeAsync<T>(Func<ILifetimeScope, Task<T>> func)
        {
            var catch_exception_and_create_scope = true;
            try
            {
                //尝试使用httpscope
                var context = HttpContext.Current;
                if (context != null)
                {
                    var s = context.GetAutofacScope();
                    //已经成功创建scope，没有必要继续尝试创建
                    catch_exception_and_create_scope = false;
                    return await func.Invoke(s);
                }
            }
            catch when (catch_exception_and_create_scope)
            {
                //do nothing
            }

            //httpcontext中没有scope，创建一次性scope
            using (var scope = Scope())
            {
                return await func.Invoke(scope);
            }
        }
    }

    public class RequestScopeModule : IHttpModule
    {
        public void Dispose()
        {
            $"{nameof(RequestScopeModule)}被销毁".AddBusinessInfoLog();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                HttpContext.Current.SetAutofacScope(AppContext.Scope());
            };

            context.EndRequest += (sender, e) =>
            {
                try
                {
                    var scope = HttpContext.Current.GetAutofacScope();
                    scope.Dispose();
                }
                catch (Exception err)
                {
                    err.AddErrorLog("销毁请求scope失败");
                }
            };
        }
    }

    /*
     1、InstancePerDependency

对每一个依赖或每一次调用创建一个新的唯一的实例。这也是默认的创建实例的方式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets a new, unique instance (default.)

2、InstancePerLifetimeScope

在一个生命周期域中，每一个依赖或调用创建一个单一的共享的实例，且每一个不同的生命周期域，实例是唯一的，不共享的。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a single ILifetimeScope gets the same, shared instance. Dependent components in different lifetime scopes will get different instances.

3、InstancePerMatchingLifetimeScope

在一个做标识的生命周期域中，每一个依赖或调用创建一个单一的共享的实例。打了标识了的生命周期域中的子标识域中可以共享父级域中的实例。若在整个继承层次中没有找到打标识的生命周期域，则会抛出异常：DependencyResolutionException。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope tagged with any of the provided tags value gets the same, shared instance. Dependent components in lifetime scopes that are children of the tagged scope will share the parent's instance. If no appropriately tagged scope can be found in the hierarchy an DependencyResolutionException is thrown.

4、InstancePerOwned

在一个生命周期域中所拥有的实例创建的生命周期中，每一个依赖组件或调用Resolve()方法创建一个单一的共享的实例，并且子生命周期域共享父生命周期域中的实例。若在继承层级中没有发现合适的拥有子实例的生命周期域，则抛出异常：DependencyResolutionException。

官方文档解释：

Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope created by an owned instance gets the same, shared instance. Dependent components in lifetime scopes that are children of the owned instance scope will share the parent's instance. If no appropriate owned instance scope can be found in the hierarchy an DependencyResolutionException is thrown.

5、SingleInstance

每一次依赖组件或调用Resolve()方法都会得到一个相同的共享的实例。其实就是单例模式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets the same, shared instance.

6、InstancePerHttpRequest

在一次Http请求上下文中,共享一个组件实例。仅适用于asp.net mvc开发。
     */
}
