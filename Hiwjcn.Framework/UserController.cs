using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using Model.Sys;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System;
using Hiwjcn.Core;
using Lib.cache;

namespace WebCore.MvcLib.Controller
{
    [ValidateInput(false)]
    public class UserBaseController : BaseController
    {
        /// <summary>
        /// 网站配置
        /// </summary>
        public List<OptionModel> Settings { get; private set; }

        public UserBaseController()
        {
            //自动登录还有问题
            //AutoLoginSSO();
            //AutoLogin();

            //加载数据库option
            LoadSettings();

            this.X.LoadLoginUser();
        }

        /// <summary>
        /// 不实用sso，直接查数据库
        /// </summary>
        [NonAction]
        private void AutoLogin()
        {
            AppContext.Scope(x =>
            {
                var logincontext = x.Resolve_<LoginStatus>();

                var loginuser = logincontext.GetLoginUser();
                if (loginuser != null) { return true; }

                var email = logincontext.GetCookieUID();
                var token = logincontext.GetCookieToken();
                if (ValidateHelper.IsAllPlumpString(email, token))
                {
                    var bll = x.Resolve_<IUserService>();
                    var model = bll.LoginByToken(email, token);
                    //如果通过token登陆成功
                    if (model != null && model.UserToken != null)
                    {
                        logincontext.SetUserLogin(loginuser: new LoginUserInfo() { });
                        return true;
                    }
                }
                logincontext.SetUserLogout();
                return true;
            });
        }

        /// <summary>
        /// 加载网站配置项
        /// </summary>
        [NonAction]
        private void LoadSettings()
        {
            AppContext.Scope(x =>
            {
                //options
                var setting = x.Resolve_<ISettingService>();
                var cache = x.Resolve_<ICacheProvider>();

                this.Settings = cache.GetOrSet(
                    CacheKeyManager.SysOptionListKey(),
                    () => setting.GetAllOptions(),
                    TimeSpan.FromMinutes(3));

                this.Settings = ConvertHelper.NotNullList(this.Settings);

                foreach (var set in this.Settings)
                {
                    ViewData[set.Key] = set.Value;
                }
                return true;
            });
        }

        /// <summary>
        /// 获取网站配置，没有值就返回空字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [NonAction]
        public string GetOption(string key)
        {
            return ConvertHelper.GetString(Settings?.FirstOrDefault(x => x.Key == key)?.Value);
        }

    }
}
