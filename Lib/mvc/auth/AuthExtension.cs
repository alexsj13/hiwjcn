using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.ioc;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;
using Autofac;
using Lib.mvc.auth.validation;

namespace Lib.mvc.auth
{
    public static class AuthExtension
    {
        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static async Task<LoginUserInfo> GetAuthUserAsync(this HttpContext context, string name = null)
        {
            using (var x = AppContext.Scope())
            {
                var loginuser = await x.Resolve_<ITokenValidationProvider>(name).GetLoginUserInfoAsync(context);

                return loginuser;
            }
        }

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static LoginUserInfo GetAuthUser(this HttpContext context, string name = null)
        {
            using (var x = AppContext.Scope())
            {
                var loginuser = x.Resolve_<ITokenValidationProvider>(name).GetLoginUserInfo(context);

                return loginuser;
            }
        }

        /// <summary>
        /// 获取bearer token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetBearerToken(this HttpContext context)
        {
            var bearer = "Bearer" + ' '.ToString();
            var token = context.Request.Headers["Authorization"] ?? string.Empty;
            if (token.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring(bearer.Length);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取bearer token或者header.auth.token
        /// </summary>
        public static string GetAuthToken(this HttpContext context)
        {
            var tk = context.GetBearerToken();
            if (!ValidateHelper.IsPlumpString(tk))
            {
                tk = context.Request.Headers["auth.token"];
            }
            return tk;
        }

        /// <summary>
        /// 获取client id
        /// </summary>
        public static string GetAuthClientID(this HttpContext context) =>
            context.Request.Headers["auth.client_id"] ?? string.Empty;

        /// <summary>
        /// 获取client security
        /// </summary>
        public static string GetAuthClientSecurity(this HttpContext context) =>
            context.Request.Headers["auth.client_security"] ?? string.Empty;

        /// <summary>
        /// 使用cookie登录
        /// </summary>
        public static void CookieLogin(this HttpContext context, LoginUserInfo loginuser)
        {
            using (var s = AppContext.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogin(context, loginuser);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void CookieLogout(this HttpContext context)
        {
            using (var s = AppContext.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogout(context);
            }
        }

        /// <summary>
        /// cookie store
        /// </summary>
        public static void AuthUseLoginStatus(this ContainerBuilder builder, Func<LoginStatus> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// 获取token client的逻辑
        /// </summary>
        public static void AuthUseValidationDataProvider<T>(this ContainerBuilder builder)
            where T : IAuthDataProvider
        {
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// auth服务器配置
        /// </summary>
        public static void AuthUseServerConfig(this ContainerBuilder builder, Func<AuthServerConfig> config)
        {
            builder.Register(_ => config.Invoke()).AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        /// <summary>
        /// 访问服务的方式，wcf，webapi，db
        /// </summary>
        public static void AuthUseServerApiAccessService<T>(this ContainerBuilder builder) where T : IAuthApi
        {
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces();
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        public static void AuthClientUseCustomValidation<T>(this ContainerBuilder builder)
            where T : ITokenValidationProvider
        {
            builder.RegisterType<T>().AsSelf().As<ITokenValidationProvider>().SingleInstance();
        }
    }
}
