using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Lib.mvc.user;
using Lib.rpc;
using Lib.mvc;
using Lib.ioc;
using System.ServiceModel;
using System.Net.Http;
using Lib.net;
using System.Runtime.Serialization;

namespace Lib.mvc.auth
{
    /// <summary>
    /// 对外提供的服务（webapi，wcf）
    /// </summary>
    public interface IAuthApi
    {
        /// <summary>
        /// 用code换token
        /// </summary>
        Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type);

        /// <summary>
        /// 用token换取登录信息
        /// </summary>
        Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token);

        /// <summary>
        /// 用验证码登录换取code
        /// </summary>
        Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms);

        /// <summary>
        /// 用密码登录换取code
        /// </summary>
        Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<_<string>> RemoveCacheAsync(CacheBundle data);
    }
}
