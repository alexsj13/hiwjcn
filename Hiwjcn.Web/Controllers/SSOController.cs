using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Framework;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using Model.User;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebLogic.Bll.User;
using System.Linq;
using WebLogic.Model.User;
using Lib.data;
using Lib.extension;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Lib.cache;
using WebCore.MvcLib.Controller;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.mvc.auth.validation;
using Hiwjcn.Core.Data;
using System.Collections.Generic;
using System.Net;
using System.Data.Entity;
using Hiwjcn.Framework.Provider;

namespace Hiwjcn.Web.Controllers
{
    public class SSOController : BaseController
    {
        private LoginStatus loginStatus => AccountHelper.SSO;

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginAction(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsAllPlumpString(username, password))
                {
                    return GetJsonRes("请输入账号密码");
                }

                using (var db = new SSODB())
                {
                    var md5 = password.ToMD5().ToUpper();
                    var model = await db.T_UserInfo.Where(x => x.UserName == username && x.PassWord == md5).FirstOrDefaultAsync();
                    if (model == null)
                    {
                        return GetJsonRes("账户密码错误");
                    }
                    if (model.IsActive <= 0 || model.IsRemove > 0)
                    {
                        return GetJsonRes("用户被删除，或者被禁用");
                    }
                    var loginuser = model.LoginUserInfo();
                    loginStatus.SetUserLogin(this.X.context, loginuser);
                    return GetJsonRes(string.Empty);
                }
            });
        }

        [RequestLog]
        public async Task<ActionResult> Login(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback, "/");
                var loginuser = await this.X.context.GetSSOLoginUserAsync();
                if (loginuser != null)
                {
                    return Redirect(url);
                }

                await Task.FromResult(1);
                return View();
            });
        }

        [RequestLog]
        public async Task<ActionResult> Logout(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);

                loginStatus.SetUserLogout(this.X.context);

                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback, "/");
                return Redirect(url);
            });
        }

        [SSOPageValid]
        public ActionResult test()
        {
            return GetJson(this.X.context.GetSSOLoginUser());
        }
    }
}