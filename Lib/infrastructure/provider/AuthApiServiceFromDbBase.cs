using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.cache;
using Lib.infrastructure.service;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.mvc.user;
using Lib.extension;
using Lib.helper;
using Lib.mvc.auth;
using Lib.data;
using Lib.core;
using System.Data.Entity;
using Lib.data.ef;

namespace Lib.infrastructure.provider
{
    public abstract class AuthApiServiceFromDbBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> :
        AuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase>, IAuthApi
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        private readonly object AuthApiServiceFromDB = new object();

        protected readonly IAuthLoginService _loginService;

        public AuthApiServiceFromDbBase(
            IAuthLoginService _loginService,
            ICacheProvider _cache,
            IEFRepository<ClientBase> _clientRepo,
            IEFRepository<ScopeBase> _scopeRepo,
            IEFRepository<TokenBase> _tokenRepo,
            IEFRepository<CodeBase> _codeRepo,
            IEFRepository<TokenScopeBase> _tokenScopeRepo) :
            base(_cache, _clientRepo, _scopeRepo, _tokenRepo, _codeRepo, _tokenScopeRepo)
        {
            this._loginService = _loginService;
        }

        public abstract Task CacheHitLog(string cache_key, CacheHitStatusEnum status);
        
        public virtual async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var res = new _<TokenModel>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAccessTokenAsync)}";
            var p = new { client_id = client_id, client_secret = client_secret, code = code, grant_type = grant_type }.ToJson();

            var data = await this.CreateTokenAsync(client_id, client_secret, code);
            if (data.error)
            {
                $"获取token异常|{data.msg}|{func}|{p}".AddBusinessInfoLog();

                res.SetErrorMsg(data.msg);
                return res;
            }
            var token = data.data;
            var token_data = new TokenModel()
            {
                Token = token.UID,
                RefreshToken = token.RefreshToken,
                Expire = token.ExpiryTime
            };

            this._cache.Remove(this.AuthUserInfoCacheKey(token.UserUID));

            res.SetSuccessData(token_data);
            return res;
        }

        public virtual async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var data = new _<LoginUserInfo>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetLoginUserInfoByTokenAsync)}";
            var p = new { client_id = client_id, access_token = access_token }.ToJson();

            if (!ValidateHelper.IsAllPlumpString(access_token, client_id))
            {
                $"验证token异常|参数为空|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("参数为空");
                return data;
            }

            var cache_expire = TimeSpan.FromMinutes(10);

            var hit_status = CacheHitStatusEnum.Hit;
            var cache_key = this.AuthTokenCacheKey(access_token);

            //查找token
            var token = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this.FindTokenAsync(client_id, access_token);
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (token == null)
            {
                $"token不存在|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("token不存在");
                return data;
            }

            hit_status = CacheHitStatusEnum.Hit;
            cache_key = this.AuthUserInfoCacheKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this._loginService.GetUserInfoByUID(token.UserUID);
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (loginuser == null)
            {
                $"用户不存在|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("用户不存在");
                return data;
            }

            loginuser.LoginToken = token.UID;
            loginuser.RefreshToken = token.RefreshToken;
            loginuser.TokenExpire = token.ExpiryTime;
            loginuser.Scopes = token.Scopes?.Select(x => x.Name).ToList();

            data.SetSuccessData(loginuser);

            return data;
        }

        public virtual async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByOneTimeCodeAsync)}";
            var p = new { client_id = client_id, scope = scopes, phone = phone, sms = sms }.ToJson();

            var loginuser = await this._loginService.LoginByCode(phone, sms);
            if (loginuser.error)
            {
                $"验证码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._scopeRepo.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public virtual async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByPasswordAsync)}";
            var p = new { client_id = client_id, scope = scopes, username = username, password = password }.ToJson();

            var loginuser = await this._loginService.LoginByPassword(username, password);
            if (loginuser.error)
            {
                $"密码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._scopeRepo.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public virtual async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            var res = new _<string>();
            if (data == null)
            {
                res.SetErrorMsg("缓存key参数为空");
                return res;
            }

            var keys = new List<string>();
            if (ValidateHelper.IsPlumpList(data.UserUID))
            {
                keys.AddRange(data.UserUID.Select(x => this.AuthUserInfoCacheKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.TokenUID))
            {
                keys.AddRange(data.TokenUID.Select(x => this.AuthTokenCacheKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.SSOUserUID))
            {
                keys.AddRange(data.SSOUserUID.Select(x => this.AuthSSOUserInfoCacheKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.ClientUID))
            {
                keys.AddRange(data.ClientUID.Select(x => this.AuthClientCacheKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.ScopeUID))
            {
                keys.AddRange(data.ScopeUID.Select(x => this.AuthScopeCacheKey(x)));
            }

            foreach (var key in keys.Distinct())
            {
                this._cache.Remove(key);
            }

            await Task.FromResult(1);

            res.SetSuccessData(string.Empty);

            return res;
        }

    }
}
