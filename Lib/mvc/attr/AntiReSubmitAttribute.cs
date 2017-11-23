using Lib.cache;
using Lib.core;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Lib.ioc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 防止重复提交
    /// </summary>
    public class AntiReSubmitAttribute : ActionFilterAttribute
    {
        public virtual int CacheSeconds { get; set; } = 5;

        public virtual string ErrorMessage { get; set; } = "重复提交，请稍后再试";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionID = filterContext.HttpContext.Session.SessionID;
            var key = $"{nameof(AntiReSubmitAttribute)}:{sessionID}";

            var reqparams = filterContext.HttpContext.Request.Form.ToDict();
            reqparams = reqparams.AddDict(filterContext.HttpContext.Request.QueryString.ToDict());

            var dict = new SortedDictionary<string, string>(reqparams, new MyStringComparer());
            var submitData = dict.ToUrlParam();
            var (AreaName, ControllerName, ActionName) = filterContext.RouteData.GetA_C_A();
            submitData = $"{AreaName}/{ControllerName}/{ActionName}/:{submitData}";
            //读取缓存
            AppContext.Scope(s =>
            {
                using (var cache = s.Resolve_<ICacheProvider>())
                {
                    var data = cache.Get<string>(key);
                    if (data.Success)
                    {
                        if (data.Result == submitData)
                        {
                            filterContext.Result = ResultHelper.BadRequest(this.ErrorMessage);
                            return true;
                        }
                    }
                    //10秒钟不能提交相同的数据
                    CacheSeconds = Math.Abs(CacheSeconds);
                    if (CacheSeconds == 0) { throw new Exception("缓存时间不能为0"); }
                    cache.Set(key, submitData, TimeSpan.FromSeconds(CacheSeconds));
                }
                return true;
            });
            base.OnActionExecuting(filterContext);
        }
    }
}
