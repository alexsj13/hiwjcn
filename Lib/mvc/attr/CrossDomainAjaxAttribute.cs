using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 跨域请求
    /// </summary>
    public class CrossDomainAjaxAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            System.Web.HttpContext.Current.AllowCrossDomainAjax();

            base.OnActionExecuted(filterContext);
        }
    }
}
