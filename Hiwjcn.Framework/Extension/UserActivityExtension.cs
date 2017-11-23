using Hiwjcn.Core.Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.extension;
using Lib.mvc;

namespace Hiwjcn.Framework.Extension
{
    public static class UserActivityExtension
    {
        public static void FromHttpContext(this UserActivity model, HttpContext context)
        { }

        public static void FromController(this UserActivity model, Controller controller)
        {
            model.FromHttpContext(HttpContext.Current);
            var data = controller.RouteData.GetA_C_A();
            model.ActionName = data.action;
            model.ControllerName = data.controller;
            model.ActionName = data.action;
        }
    }
}
