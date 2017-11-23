using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Lib.mvc.user;
using Lib.extension;
using System.Configuration;

namespace Lib.mvc.attr
{
    public class HideActionAttribute : _ActionFilterBaseAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var show = (ConfigurationManager.AppSettings["show_api"] ?? "false").ToBool();
            if (!show)
            {
                filterContext.Result = this.GetJson(new _() { success = false, msg = "not for this site" });
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
