using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.core;
using Lib.helper;
using Lib.ioc;

namespace Lib.mvc
{
    /// <summary>
    /// 这个暂时不用了
    /// </summary>
    public class AutoFacControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// 优先使用spring创建控制器
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            string area = ConvertHelper.GetString(requestContext.RouteData.DataTokens["area"]).ToLower();
            string cname = ConvertHelper.GetString(controllerName).ToLower();

            var key = $"{area}-{cname}";
            var controller = AppContext.Scope(x => x.Resolve_<IController>(key));
            return controller;
        }

        /// <summary>
        /// 释放控制器
        /// </summary>
        /// <param name="controller"></param>
        public override void ReleaseController(IController controller)
        {
            base.ReleaseController(controller);
        }
    }
}
