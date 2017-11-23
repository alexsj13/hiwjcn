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
using Model.User;
using Lib.events;
using Lib.cache;
using System.Configuration;
using Hiwjcn.Core;

namespace Hiwjcn.Bll.Auth
{
    /// <summary>
    /// 用来清理缓存等
    /// </summary>
    public class TokenEventHandler :
        IConsumer<EntityInserted<AuthToken>>,
        IConsumer<EntityDeleted<AuthToken>>,
        IConsumer<EntityUpdated<AuthToken>>,
        IConsumer<EntityInserted<AuthScope>>,
        IConsumer<EntityDeleted<AuthScope>>,
        IConsumer<EntityUpdated<AuthScope>>,
        IConsumer<EntityInserted<AuthClient>>,
        IConsumer<EntityDeleted<AuthClient>>,
        IConsumer<EntityUpdated<AuthClient>>
    {
        private readonly ICacheProvider _cache;
        public TokenEventHandler(ICacheProvider _cache)
        {
            this._cache = _cache;
        }

        public void HandleEvent(EntityUpdated<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityInserted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityDeleted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityInserted<AuthScope> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthScopeAllKey());
            this._cache.Remove(CacheKeyManager.AuthScopeKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityDeleted<AuthScope> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthScopeAllKey());
            this._cache.Remove(CacheKeyManager.AuthScopeKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityUpdated<AuthScope> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthScopeAllKey());
            this._cache.Remove(CacheKeyManager.AuthScopeKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityInserted<AuthClient> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthClientKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityDeleted<AuthClient> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthClientKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityUpdated<AuthClient> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthClientKey(eventMessage.Entity?.UID));
        }
    }
}
