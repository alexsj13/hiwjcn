using Lib.cache;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Lib.extension;
using Lib.ioc;
using System.Web;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 缓存页面html
    /// 参考OutputCache
    /// </summary>
    public class HtmlCacheAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int Minute { get; set; }

        /// <summary>
        /// 是否是从缓存中读取的数据
        /// </summary>
        private bool ReadFromCache = false;

        /// <summary>
        /// 访问页面URL，作为key使用
        /// </summary>
        private string URL { get; set; }

        /// <summary>
        /// 缓存页面html，时间是分钟
        /// </summary>
        /// <param name="minute"></param>
        public HtmlCacheAttribute(int minute = 5)
        {
            this.Minute = minute;
        }

        #region 执行action前，尝试读取缓存html
        /// <summary>
        /// action执行中
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //URL作为key
            URL = ConvertHelper.GetString(filterContext.HttpContext.Request.Url);

            var data = AppContext.Scope(x => x.Resolve_<ICacheProvider>().Get<string>(URL));

            if (data.Success)
            {
                ReadFromCache = true;
                filterContext.Result = new ContentResult()
                {
                    Content = data.Result,
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "text/html"
                };
                return;
            }
            else
            {
                //attribute好像只实例化了一次，这里必须手动设置，不然会用上一次的值
                ReadFromCache = false;
            }

            base.OnActionExecuting(filterContext);
        }
        #endregion

        #region 执行action后，如果不是从缓存读取的数据就加入缓存
        /// <summary>
        /// 结果执行完毕
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// 注意：这个方法在渲染view后执行，
        /// 在这个方法里将重新渲染view拿到html（因为不知道怎么拿到之前渲染好的html）
        /// 如果在前一次渲染view的时候修改viewdata中的数据
        /// 将会导致最终被缓存的内容与第一次渲染的内容不一致
        /// 
        /// 重要的事情说三遍【不要修改viewdata中的数据，要修改请重新创建对象】
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //如果不是从缓存中读取的就添加到缓存
            if (!ReadFromCache)
            {
                var context = HttpContext.Current;
                //获取view的名称
                string viewName = filterContext.RouteData.GetRequiredString("action");
                viewName = ConvertHelper.GetString(viewName);
                using (var sw = new StringWriter())
                {
                    //找到view
                    var view = System.Web.Mvc.ViewEngines.Engines.FindView(
                        filterContext.Controller.ControllerContext, viewName, string.Empty).View;
                    //初始化view上下文
                    var vc = new ViewContext(
                        filterContext.Controller.ControllerContext,
                        view,
                        filterContext.Controller.ViewData,
                        filterContext.Controller.TempData,
                        sw);
                    //渲染view
                    view.Render(vc, sw);
                    //获取渲染后的html
                    var html = sw.ToString();
                    //设置缓存
                    var cache = context.AutofacRequestLifetimeScope().Resolve_<ICacheProvider>();
                    cache.Set(URL, html, TimeSpan.FromMinutes(Minute));
                }
            }

            base.OnResultExecuted(filterContext);
        }
        #endregion

    }
}
