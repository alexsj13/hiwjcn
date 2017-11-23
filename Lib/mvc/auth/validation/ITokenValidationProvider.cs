using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.ioc;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.mvc.user;
using System.Net;
using System.Net.Http;
using Lib.net;

namespace Lib.mvc.auth.validation
{
    public interface ITokenValidationProvider
    {
        LoginUserInfo GetLoginUserInfo(HttpContext context);

        Task<LoginUserInfo> GetLoginUserInfoAsync(HttpContext context);
    }

    /// <summary>
    /// 拿到token和client信息后去验证信息，并拿到用户信息
    /// </summary>
    public abstract class TokenValidationProviderBase : ITokenValidationProvider
    {
        /// <summary>
        /// 缓存一个请求中的登录用户信息
        /// </summary>
        public abstract string HttpContextItemKey();

        public abstract LoginUserInfo FindUser(HttpContext context);

        public abstract Task<LoginUserInfo> FindUserAsync(HttpContext context);

        public virtual void WhenUserNotLogin(HttpContext context)
        {
            using (var s = AppContext.Scope())
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogout(context);
            }
        }

        public virtual void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            using (var s = AppContext.Scope())
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogin(context, loginuser);
            }
        }

        LoginUserInfo ITokenValidationProvider.GetLoginUserInfo(HttpContext context)
        {
            var data = context.CacheInHttpContext(this.HttpContextItemKey(), () =>
            {
                var loginuser = this.FindUser(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

        async Task<LoginUserInfo> ITokenValidationProvider.GetLoginUserInfoAsync(HttpContext context)
        {
            var data = await context.CacheInHttpContextAsync(this.HttpContextItemKey(), async () =>
            {
                var loginuser = await this.FindUserAsync(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

    }
}
