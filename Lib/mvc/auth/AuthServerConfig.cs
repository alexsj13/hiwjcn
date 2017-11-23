using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.mvc.user;
using Lib.mvc.auth;
using Lib.mvc.auth.validation;
using Lib.mvc;
using System.Configuration;

namespace Lib.mvc.auth
{
    public static class WebClientConfig
    {
        private static readonly Lazy<string> id = new Lazy<string>(() =>
        (ConfigurationManager.AppSettings["auth.client_id"] ?? throw new Exception("没有配置auth client id")).ToMD5().ToLower());
        private static readonly Lazy<string> security = new Lazy<string>(() =>
        (ConfigurationManager.AppSettings["auth.client_security"] ?? throw new Exception("没有配置auth client security")).ToMD5().ToLower());

        public static string ClientID() => id.Value;
        public static string ClientSecurity() => security.Value;
    }

    public static class AuthApiControllerConfig
    {
        public const string ControllerName = "Auth";
        public const string Action_AuthCodeByPassword = "AuthCodeByPassword";
        public const string Action_AuthCodeByOneTimeCode = "AuthCodeByOneTimeCode";
        public const string Action_AccessToken = "AccessToken";
        public const string Action_CheckToken = "CheckToken";
        public const string Action_RemoveCache = "RemoveCache";
    }

    public class AuthServerConfig
    {
        public readonly string ServerUrl;
        public readonly string WcfUrl;

        public AuthServerConfig(string host) :
            this(host, $"{host?.EnsureTrailingSlash()}Service/AuthApiService.svc")
        {
            //
        }

        public AuthServerConfig(string host, string wcf_path)
        {
            void EnsureUrl(string url)
            {
                var lower = (url ?? throw new Exception("auth服务器地址不能为空")).ToLower();
                var prefix_ok = lower.StartsWith("http://") || lower.StartsWith("https://");
                if (!prefix_ok)
                {
                    throw new Exception("服务器地址必须以http或者https开头");
                }
            }

            //server root
            this.ServerUrl = ConvertHelper.GetString(host);
            EnsureUrl(this.ServerUrl);

            //server wcf url
            this.WcfUrl = wcf_path;
            EnsureUrl(this.WcfUrl);
        }

        public string ApiPath(params string[] paths)
        {
            var path = "/".Join_(paths.Where(x => ValidateHelper.IsPlumpString(x)));
            return ServerUrl.EnsureTrailingSlash() + path;
        }

        public string AuthApiPath(string action) => this.ApiPath(new string[] { AuthApiControllerConfig.ControllerName, action });

        /// <summary>
        /// 获取code
        /// </summary>
        public string CreateAuthCodeByPassword() => this.AuthApiPath(AuthApiControllerConfig.Action_AuthCodeByPassword);

        /// <summary>
        /// 获取code
        /// </summary>
        public string CreateCodeByOneTimeCode() => this.AuthApiPath(AuthApiControllerConfig.Action_AuthCodeByOneTimeCode);

        /// <summary>
        /// 获取token
        /// </summary>
        public string CreateToken() => this.AuthApiPath(AuthApiControllerConfig.Action_AccessToken);

        /// <summary>
        /// token换用户信息
        /// </summary>
        public string CheckToken() => this.AuthApiPath(AuthApiControllerConfig.Action_CheckToken);

        /// <summary>
        /// 删除token缓存
        /// </summary>
        /// <returns></returns>
        public string RemoveCache() => this.AuthApiPath(AuthApiControllerConfig.Action_RemoveCache);

    }

}
