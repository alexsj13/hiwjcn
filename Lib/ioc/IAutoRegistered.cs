using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.ioc
{
    /// <summary>
    /// autofac 自动查找注册
    /// </summary>
    public interface IAutoRegistered { }

    /// <summary>
    /// 单例
    /// </summary>
    public class SingleInstanceAttribute : Attribute { }

    /// <summary>
    /// 拦截
    /// </summary>
    public class InterceptInstanceAttribute : Attribute { }

    /// <summary>
    /// 不注册IOC
    /// </summary>
    public class NotRegIocAttribute : Attribute { }
}
