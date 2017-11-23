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
using Lib.extension;
using Lib.mvc.auth;
using Lib.mvc.attr;

namespace Lib.mvc.user
{
    [Obsolete("已过时，使用ValidLoginBaseAttribute")]
    public abstract class SSOCheckAttribute : ValidLoginBaseAttribute
    {
        /// <summary>
        /// 没有权限跳转的URL
        /// </summary>
        public string NoPermissionUrl { get; set; }

        /// <summary>
        /// 跳回地址
        /// </summary>
        public string SpecifyedContinueUrl { get; set; } = ConfigurationManager.AppSettings["SSO_CONTINUE_URL"];
        public bool NoLoginResultAsInterface { get; set; } = ConvertHelper.GetString(ConfigurationManager.AppSettings["SSO_NO_LOGIN_RESULT_FOR_INTERFACE"]).ToBool();

        public SSOCheckAttribute()
        {
            SSOClientHelper.CheckSSOConfig();
        }

        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return AccountHelper.SSO.GetLoginUser();
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            var context = HttpContext.Current;
            var continue_url = this.SpecifyedContinueUrl;
            if (ValidateHelper.IsPlumpString(continue_url))
            {
                continue_url = RequestHelper.GetBaseUrl(context.Request) + continue_url;
            }
            else
            {
                continue_url = RequestHelper.GetCurrentUrl(context.Request);
            }
            var login_url = SSOClientHelper.BuildSSOLoginUrl(continue_url);
            if (this.NoLoginResultAsInterface)
            {
                filterContext.Result = GetJson(new _()
                {
                    success = false,
                    msg = "未登录",
                    data = new { sso_login = login_url },
                    code = "-999"
                });
            }
            else
            {
                filterContext.Result = new RedirectResult(login_url);
            }
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            if (NoPermissionUrl?.Length > 0)
            {
                filterContext.Result = new RedirectResult(NoPermissionUrl);
            }
            else
            {
                filterContext.Result = GetJson(new _() { success = false, msg = "没有权限" });
            }
        }

    }
}
