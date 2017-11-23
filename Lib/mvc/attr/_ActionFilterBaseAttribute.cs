using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Lib.ioc;
using Lib.mvc.user;
using System.Web.SessionState;

namespace Lib.mvc.attr
{
    public abstract class _ActionFilterBaseAttribute : ActionFilterAttribute
    {
        protected ActionResult GetJson(object data)
        {
            return new JsonResult()
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
