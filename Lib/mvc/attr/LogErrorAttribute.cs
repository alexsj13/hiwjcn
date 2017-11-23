using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 异常处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class LogErrorAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// 处理异常 404 500。。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                var ex = filterContext.Exception;
                if (ex != null && !filterContext.ExceptionHandled)
                {
                    ex.AddErrorLog("全局捕捉到错误");
                }
                //filterContext.ExceptionHandled = true;
            }
            base.OnException(filterContext);
        }
    }
}
