using Lib.helper;
using Lib.mvc.user;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.extension;
using Lib.mvc.auth.validation;
using Lib.ioc;
using Hiwjcn.Core;
using Lib.cache;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Framework.Provider
{
    public static class SSOHelper
    {
        public static LoginStatus LoginStatus() => AccountHelper.SSO;
    }

    /// <summary>
    /// sso登录验证
    /// </summary>
    public class SSOValidationProvider : TokenValidationProviderBase
    {
        private readonly LoginStatus ls;

        public SSOValidationProvider()
        {
            this.ls = SSOHelper.LoginStatus();
        }

        public override string HttpContextItemKey() => "httpcontext.item.sso.user.entity";

        public override LoginUserInfo FindUser(HttpContext context)
        {
            try
            {
                var uid = ls.GetCookieUID(context);
                var token = ls.GetCookieToken(context);
                if (!ValidateHelper.IsAllPlumpString(uid, token))
                {
                    return null;
                }

                var user = AppContext.Scope(s =>
                {
                    var key = CacheKeyManager.AuthSSOUserInfoKey(uid);
                    var cache = s.Resolve_<ICacheProvider>();
                    return cache.GetOrSet(key, () =>
                    {
                        using (var db = new SSODB())
                        {
                            var model = db.T_UserInfo.Where(x => x.UID == uid).FirstOrDefault();
                            if (model == null)
                            {
                                return null;
                            }
                            //load permission
                            //这里只拿了角色关联的权限，部门关联的权限没有拿
                            var roleslist = db.Auth_UserRole.Where(x => x.UserID == uid)
                                .Select(x => x.RoleID).ToList()
                                .Select(x => $"role:{x}").ToList();

                            model.Permissions = db.Auth_PermissionMap.Where(x => roleslist.Contains(x.MapKey))
                                .Select(x => x.PermissionID).ToList()
                                .Distinct().ToList();

                            return model;
                        }
                    }, TimeSpan.FromSeconds(60));
                });

                if (user == null || user.CreateToken() != token)
                {
                    return null;
                }

                return user.LoginUserInfo();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            return await Task.FromResult(this.FindUser(context));
        }

        public override void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            ls.SetUserLogin(context, loginuser);
        }

        public override void WhenUserNotLogin(HttpContext context)
        {
            ls.SetUserLogout(context);
        }
    }

    public static class SSOExtension
    {
        public static LoginUserInfo GetSSOLoginUser(this HttpContext context)
        {
            return ((ITokenValidationProvider)new SSOValidationProvider()).GetLoginUserInfo(context);
        }

        public static async Task<LoginUserInfo> GetSSOLoginUserAsync(this HttpContext context)
        {
            return await ((ITokenValidationProvider)new SSOValidationProvider()).GetLoginUserInfoAsync(context);
        }

        public static LoginUserInfo LoginUserInfo(this T_UserInfo model)
        {
            return new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                UserName = model.UserName,
                NickName = model.UserName,
                LoginToken = model.CreateToken(),
                TokenExpire = DateTime.Now.AddDays(30),
                Permissions = model.Permissions?.ToList()
            };
        }

        public static void SSOLogin(this HttpContext context, LoginUserInfo loginuser)
        {
            SSOHelper.LoginStatus().SetUserLogin(context, loginuser);
        }

        public static void SSOLogout(this HttpContext context)
        {
            SSOHelper.LoginStatus().SetUserLogout(context);
        }
    }

    public class SSOPageValidAttribute : ValidLoginBaseAttribute
    {
        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return HttpContext.Current.GetSSOLoginUser();
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = new ViewResult() { ViewName = "~/Views/Shared/Limited.cshtml" };
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            var current_url = filterContext.HttpContext.Request.Url.ToString();
            current_url = EncodingHelper.UrlEncode(current_url);
            filterContext.Result = new RedirectResult($"/sso/login?continue={current_url}");
        }
    }

    public class SSOApiValidAttribute : ValidLoginBaseAttribute
    {
        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return HttpContext.Current.GetSSOLoginUser();
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _() { success = false, msg = "没有权限", code = (-(int)HttpStatusCode.Unauthorized).ToString() });
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _() { success = false, msg = "没有登录", code = (-999).ToString() });
        }
    }
}
