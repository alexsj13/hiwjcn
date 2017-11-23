using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.ioc
{
    /// <summary>
    /// 注册依赖
    /// </summary>
    public class BaseDependencyRegistrar : IDependencyRegistrar
    {
        public void Clean()
        {
            //do nothing
        }

        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="builder"></param>
        public void Register(ref ContainerBuilder builder)
        {
            //
        }
    }
}
