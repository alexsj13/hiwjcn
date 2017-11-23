using Hiwjcn.Core.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;
using Lib.events;
using System.Configuration;
using Lib.mvc;
using Lib.cache;
using Hiwjcn.Core;
using Lib.infrastructure.entity;
using Lib.data.ef;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenService : ServiceBase<AuthToken>, IAuthTokenService
    {
        private readonly IEFRepository<AuthToken> _AuthTokenRepository;
        private readonly IEFRepository<AuthTokenScope> _AuthTokenScopeRepository;
        private readonly IEFRepository<AuthScope> _AuthScopeRepository;
        private readonly IEFRepository<AuthCode> _AuthCodeRepository;
        private readonly IEFRepository<AuthClient> _AuthClientRepository;
        private readonly ICacheProvider _cache;

        private readonly IEventPublisher _publisher;

        public AuthTokenService(
            IEventPublisher _publisher,
            IEFRepository<AuthToken> _AuthTokenRepository,
            IEFRepository<AuthTokenScope> _AuthTokenScopeRepository,
            IEFRepository<AuthScope> _AuthScopeRepository,
            IEFRepository<AuthCode> _AuthCodeRepository,
            IEFRepository<AuthClient> _AuthClientRepository,
            ICacheProvider _cache)
        {
            this._publisher = _publisher;
            this._cache = _cache;

            this._AuthTokenRepository = _AuthTokenRepository;
            this._AuthTokenScopeRepository = _AuthTokenScopeRepository;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthCodeRepository = _AuthCodeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        private async Task ClearOldCode(string user_uid)
        {
            var expire = DateTime.Now.AddMinutes(-TokenConfig.CodeExpireMinutes);
            await this._AuthCodeRepository.DeleteWhereAsync(x => x.UserUID == user_uid && x.CreateTime < expire);
        }

        public virtual async Task<_<AuthCode>> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid)
        {
            var data = new _<AuthCode>();

            if (!ValidateHelper.IsPlumpList(scopes))
            {
                data.SetErrorMsg("scopes为空");
                return data;
            }
            var now = DateTime.Now;

            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid && x.IsRemove <= 0);
            if (client == null)
            {
                data.SetErrorMsg("授权的应用已被删除");
                return data;
            }

            var border = now.GetDateBorder();

            var count = await this._AuthCodeRepository.GetCountAsync(x =>
              x.ClientUID == client_uid &&
              x.UserUID == user_uid &&
              x.CreateTime >= border.start &&
              x.CreateTime < border.end);

            if (count >= TokenConfig.MaxCodeCreatedDaily)
            {
                data.SetErrorMsg("授权过多");
                return data;
            }

            var code = new AuthCode()
            {
                UserUID = user_uid,
                ClientUID = client_uid,
                ScopesJson = scopes.ToJson()
            };
            code.Init("code");

            if (await this._AuthCodeRepository.AddAsync(code) > 0)
            {
                //clear old data
                await this.ClearOldCode(user_uid);

                data.SetSuccessData(code);
                return data;
            }
            throw new MsgException("保存票据失败");
        }

        private async Task ClearOldToken(string user_uid)
        {
            var now = DateTime.Now;
            await this._AuthTokenRepository.DeleteWhereAsync(x => x.UserUID == user_uid && x.ExpiryTime < now);
        }

        public virtual async Task<_<AuthToken>> CreateTokenAsync(
            string client_id, string client_secret, string code_uid)
        {
            var data = new _<AuthToken>();
            var now = DateTime.Now;

            //code
            var expire = now.AddMinutes(-TokenConfig.CodeExpireMinutes);
            var code = await this._AuthCodeRepository.GetFirstAsync(x =>
            x.UID == code_uid &&
            x.ClientUID == client_id &&
            x.CreateTime > expire &&
            x.IsRemove <= 0);
            if (code == null)
            {
                data.SetErrorMsg("code已失效");
                return data;
            }
            //client
            var client = await this._AuthClientRepository.GetFirstAsync(x =>
            x.UID == client_id &&
            x.ClientSecretUID == client_secret &&
            x.IsRemove <= 0);
            if (client == null)
            {
                data.SetErrorMsg("应用不存在");
                return data;
            }
            //scope
            var scope_names = code.ScopesJson.JsonToEntity<List<string>>(throwIfException: false);
            if (!ValidateHelper.IsPlumpList(scope_names))
            {
                data.SetErrorMsg("scope为空");
                return data;
            }
            var scopes = await this._AuthScopeRepository.GetListAsync(x => scope_names.Contains(x.Name) && x.IsRemove <= 0);
            if (scopes.Count != scope_names.Count)
            {
                data.SetErrorMsg("scope数据存在错误");
                return data;
            }

            //create new token
            var token = new AuthToken()
            {
                ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays),
                RefreshToken = Com.GetUUID(),
                ScopesInfoJson = scopes.Select(x => new { uid = x.UID, name = x.Name }).ToJson(),
                ClientUID = code.ClientUID,
                UserUID = code.UserUID
            };
            token.Init("token");
            if (!token.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._AuthTokenRepository.AddAsync(token) <= 0)
            {
                data.SetErrorMsg("保存token失败");
                return data;
            }
            //scope map
            var scope_list = scopes.Select(x =>
            {
                var s = new AuthTokenScope()
                {
                    ScopeUID = x.UID,
                    TokenUID = token.UID,
                };
                s.Init("token-scope");
                return s;
            }).ToArray();
            if (await this._AuthTokenScopeRepository.AddAsync(scope_list) <= 0)
            {
                data.SetErrorMsg("保存scope失败");
                return data;
            }

            //clear old token
            await ClearOldToken(code.UserUID);

            data.SetSuccessData(token);
            return data;
        }

        private async Task<string> DeleteTokensAsync(List<AuthToken> list)
        {
            if (!ValidateHelper.IsPlumpList(list))
            {
                return this.SUCCESS;
            }
            var msg = this.SUCCESS;
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var token_query = db.Set<AuthToken>();

                var token_uid_list = list.Select(x => x.UID).ToList();
                var user_uid_list = list.Select(x => x.UserUID).ToList();

                token_query.RemoveRange(token_query.Where(x => token_uid_list.Contains(x.UID)));

                if (await db.SaveChangesAsync() <= 0)
                {
                    msg = "删除token失败";
                    return false;
                }

                foreach (var token in token_uid_list)
                {
                    this._cache.Remove(CacheKeyManager.AuthTokenKey(token));
                }
                foreach (var user_uid in user_uid_list)
                {
                    this._cache.Remove(CacheKeyManager.AuthUserInfoKey(user_uid));
                }
                return true;
            });
            return msg;
        }

        public async Task<string> DeleteUserTokensAsync(string client_uid, string user_uid)
        {
            if (!ValidateHelper.IsPlumpString(user_uid))
            {
                return "用户ID为空";
            }
            var max_count = 500;

            var token_to_delete = await this._AuthTokenRepository.GetListAsync(x => x.ClientUID == client_uid && x.UserUID == user_uid, count: max_count);

            var errors = await this.DeleteTokensAsync(token_to_delete);
            if (ValidateHelper.IsPlumpString(errors))
            {
                return errors;
            }
            else
            {
                if (max_count == token_to_delete.Count)
                {
                    return "要删除的记录数比较多，请多试几次，直到完全删除";
                }
                return this.SUCCESS;
            }
        }

        public async Task<string> DeleteUserTokensAsync(string user_uid)
        {
            if (!ValidateHelper.IsPlumpString(user_uid))
            {
                return "用户ID为空";
            }

            var max_count = 500;

            var token_to_delete = await this._AuthTokenRepository.GetListAsync(x => x.UserUID == user_uid, count: max_count);

            var errors = await this.DeleteTokensAsync(token_to_delete);
            if (ValidateHelper.IsPlumpString(errors))
            {
                return errors;
            }
            else
            {
                if (max_count == token_to_delete.Count)
                {
                    return "要删除的记录数比较多，请多试几次，直到完全删除";
                }
                return this.SUCCESS;
            }
        }

        private async Task<bool> RefreshToken(AuthToken tk)
        {
            var success = false;
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>();
                var token = await token_query.Where(x => x.UID == tk.UID && x.ExpiryTime > now && x.IsRemove <= 0).FirstOrDefaultAsync();

                if (token != null)
                {
                    //refresh expire time
                    token.ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays);
                    token.UpdateTime = now;
                    token.RefreshTime = now;

                    success = await db.SaveChangesAsync() > 0;
                }

                return true;
            });
            return success;
        }

        public async Task<AuthToken> FindTokenAsync(string client_uid, string token_uid)
        {
            var token = default(AuthToken);
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>().AsNoTrackingQueryable();
                token = await token_query.Where(x =>
                x.UID == token_uid &&
                x.ClientUID == client_uid &&
                x.ExpiryTime > now &&
                x.IsRemove <= 0).FirstOrDefaultAsync();

                if (token != null)
                {
                    //对于scope不去排除isremove，这个条件在授权的时候已经拦截了
                    var scope_uids = db.Set<AuthTokenScope>().AsNoTrackingQueryable().Where(x => x.TokenUID == token.UID).Select(x => x.ScopeUID);
                    var scope_query = db.Set<AuthScope>().AsNoTrackingQueryable();
                    token.Scopes = (await scope_query.Where(x => scope_uids.Contains(x.UID)).ToListAsync())
                    .Select(x => (Lib.infrastructure.entity.AuthScopeBase)x).ToList();

                    //自动刷新过期时间
                    if ((token.ExpiryTime - now).TotalDays < (TokenConfig.TokenExpireDays / 2.0))
                    {
                        if (!await this.RefreshToken(token))
                        {
                            $"自动刷新token失败:{token.ToJson()}".AddBusinessInfoLog();
                        }
                    }
                }
                return true;
            });
            return token;
        }

        /// <summary>
        /// 直接获取client
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public async Task<PagerData<AuthClient>> GetMyAuthorizedClientsAsync(string user_id, string q, int page, int pagesize)
        {
            var data = new PagerData<AuthClient>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;

                var client_query = db.Set<AuthClient>().AsNoTrackingQueryable();
                var token_query = db.Set<AuthToken>().AsNoTrackingQueryable();

                var client_uids = token_query.Where(x => x.UserUID == user_id && x.ExpiryTime > now && x.IsRemove <= 0).Select(x => x.ClientUID);

                client_query = client_query.Where(x => client_uids.Contains(x.UID) && x.IsRemove <= 0);
                if (ValidateHelper.IsPlumpString(q))
                {
                    client_query = client_query.Where(x => x.ClientName.Contains(q) || x.Description.Contains(q));
                }

                data.ItemCount = await client_query.CountAsync();
                client_query = client_query.OrderByDescending(x => x.IsOfficial).OrderBy(x => x.ClientName).QueryPage(page, pagesize);
                data.DataList = await client_query.ToListAsync();

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    //
                }

                return true;
            });

            return data;
        }
    }
}
