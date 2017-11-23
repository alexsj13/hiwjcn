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

namespace Lib.mvc.auth.api
{
    /// <summary>
    /// 基于web api远程调用的auth api实现
    /// </summary>
    [Obsolete("使用wcf的provider，不提供webapi的支持")]
    public class AuthApiFromWebApi : IAuthApi
    {
        private static readonly HttpClient client = HttpClientManager.Instance.DefaultClient;

        private readonly AuthServerConfig _server;
        public AuthApiFromWebApi(AuthServerConfig _server)
        {
            this._server = _server;
        }

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var response = await client.PostAsJsonAsync(this._server.CreateToken(), new
            {
                client_id = client_id,
                client_secret = client_secret,
                code = code,
                grant_type = grant_type
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var token = json.JsonToEntity<_<TokenModel>>();
                return token;
            }
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms)
        {
            var response = await client.PostAsJsonAsync(this._server.CreateCodeByOneTimeCode(), new
            {
                client_id = client_id,
                scope = (scopes ?? new List<string>() { }).ToJson(),
                phone = phone,
                sms = sms
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var code = json.JsonToEntity<_<string>>();
                return code;
            }
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password)
        {
            var response = await client.PostAsJsonAsync(this._server.CreateAuthCodeByPassword(), new
            {
                client_id = client_id,
                scope = (scopes ?? new List<string>() { }).ToJson(),
                username = username,
                password = password
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var code = json.JsonToEntity<_<string>>();
                return code;
            }
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var response = await client.PostAsJsonAsync(this._server.CheckToken(), new
            {
                client_id = client_id,
                access_token = access_token
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var loginuser = json.JsonToEntity<_<LoginUserInfo>>();
                return loginuser;
            }
        }

        public async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            var response = await client.PostAsJsonAsync(this._server.RemoveCache(), new
            {
                data = (data ?? throw new ArgumentNullException(nameof(data))).ToJson()
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var res = json.JsonToEntity<_<string>>();
                return res;
            }
        }
    }
}
