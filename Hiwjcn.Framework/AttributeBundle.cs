using Bll.Category;
using Hiwjcn.Bll.Sys;
using Hiwjcn.Core.Model.Sys;
using Lib.extension;
using Lib.helper;
using Lib.events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using Lib.ioc;
using Lib.cache;
using Lib.mvc;
using Hiwjcn.Framework.Actors;
using System.Web;
using Akka.Actor;
using Lib.distributed.akka;

namespace Hiwjcn.Framework
{
    /// <summary>
    /// 在action之前加载导航数据
    /// </summary>
    public class LoadNavigationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// 在action之前加载导航数据
        /// </summary>
        /// <param name="cache"></param>
        public LoadNavigationAttribute(bool cache = true)
        {
            UseCache = cache;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nav_key = "nav_list";

            AppContext.Scope(x =>
            {
                if (UseCache)
                {
                    var cache = x.Resolve_<ICacheProvider>();
                    var data = cache.GetOrSet("nav_list_cache".WithCacheKeyPrefix(), () =>
                    {
                        return new CategoryBll().GetCategoryByType(nav_key);
                    }, TimeSpan.FromMinutes(3));
                    filterContext.Controller.ViewData[nav_key] = data;
                }
                else
                {
                    var data = new CategoryBll().GetCategoryByType(nav_key);
                    filterContext.Controller.ViewData[nav_key] = data;
                }

                return true;
            });

            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// 记录请求
    /// </summary>
    public class RequestLogAttribute : ActionFilterAttribute
    {
        private DateTime start { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            start = DateTime.Now;
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var context = HttpContext.Current;

                var model = new ReqLogModel();

                model.ReqTime = (DateTime.Now - start).TotalMilliseconds;
                model.ReqID = Com.GetRequestID();

                model.ReqRefURL = ConvertHelper.GetString(context.Request.UrlReferrer);
                model.ReqURL = ConvertHelper.GetString(context.Request.Url);

                var route = filterContext.RouteData.GetA_C_A();

                model.AreaName = route.area?.ToLower();
                model.ControllerName = route.controller?.ToLower();
                model.ActionName = route.action?.ToLower();

                model.ReqMethod = filterContext.HttpContext.Request.HttpMethod;

                model.PostParams = context.Request.Form.ToDict().ToUrlParam();
                model.GetParams = context.Request.QueryString.ToDict().ToUrlParam();

                ActorsManager<LogRequestActor>.Instance.DefaultClient.Tell(model);

                //Com.Range(3).ForEach_(x => AkkaHelper<TestActor>.Tell($"测试actor内存占用{DateTime.Now}"));
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
