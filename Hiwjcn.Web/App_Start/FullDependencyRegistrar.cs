using Autofac;
using Autofac.Integration.Mvc;
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

namespace Hiwjcn.Web.App_Start
{
    public class FullDependencyRegistrar : DependencyRegistrarBase
    {
        public override void Register(ref ContainerBuilder builder)
        {
            //注册控制器
            //RegController(ref builder);
            //builder.RegisterControllers(tps.web.Assembly);
            var pluginAssemblies = FindPluginAssemblies();
            pluginAssemblies.Add(this.GetType().Assembly);
            RegController(ref builder, pluginAssemblies.ToArray());
        }
    }
}