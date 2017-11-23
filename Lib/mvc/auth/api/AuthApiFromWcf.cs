using Lib.helper;
using Lib.mvc.user;
using Lib.rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.auth.api
{
    /// <summary>
    /// wcf服务的约定，把这个发布到nuget用于调用
    /// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IAuthApiService”。
    /// !!!!!!!!!!!!!!!!!!!!!!!!!
    /// 方法名不要以async结尾
    /// 方法和IAuthApi基本保持一致，这个主要是给wcf client使用的
    /// !!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    [ServiceContract]
    public interface IAuthApiWcfServiceContract
    {
        [OperationContract]
        Task<_<TokenModel>> GetAccessToken(string client_id, string client_secret, string code, string grant_type);

        [OperationContract]
        Task<_<LoginUserInfo>> GetLoginUserInfoByToken(string client_id, string access_token);

        [OperationContract]
        Task<_<string>> GetAuthCodeByOneTimeCode(string client_id, List<string> scopes, string phone, string sms);

        [OperationContract]
        Task<_<string>> GetAuthCodeByPassword(string client_id, List<string> scopes, string username, string password);

        [OperationContract]
        Task<_<string>> RemoveCache(CacheBundle data);
    }


    /// <summary>
    /// 基于wcf远程调用的auth api实现
    /// </summary>
    public class AuthApiFromWcf : IAuthApi
    {
        private readonly string url;
        public AuthApiFromWcf(AuthServerConfig _server)
        {
            this.url = _server.WcfUrl;
            if (!ValidateHelper.IsPlumpString(this.url))
            {
                throw new Exception($"{nameof(IAuthApiWcfServiceContract)}没有配置远程地址");
            }
        }

        private ServiceClient<IAuthApiWcfServiceContract> Client() => 
            new ServiceClient<IAuthApiWcfServiceContract>(this.url);

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            using (var client = this.Client())
            {
                return await client.Instance.GetAccessToken(client_id, client_secret, code, grant_type);
            }
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms)
        {
            using (var client = this.Client())
            {
                return await client.Instance.GetAuthCodeByOneTimeCode(client_id, scopes, phone, sms);
            }
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password)
        {
            using (var client = this.Client())
            {
                return await client.Instance.GetAuthCodeByPassword(client_id, scopes, username, password);
            }
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            using (var client = this.Client())
            {
                return await client.Instance.GetLoginUserInfoByToken(client_id, access_token);
            }
        }

        public async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            using (var client = this.Client())
            {
                return await client.Instance.RemoveCache(data);
            }
        }
    }
}
