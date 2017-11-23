using Lib.cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core
{
    /// <summary>
    /// 缓存key的统一管理
    /// </summary>
    public static class CacheKeyManager
    {
        public static string AuthTokenKey(string token_uid) =>
            $"auth.token.uid={token_uid}".WithCacheKeyPrefix();

        public static string AuthClientKey(string client_uid) =>
            $"auth.client.uid={client_uid}".WithCacheKeyPrefix();

        public static string AuthScopeKey(string scope_uid) =>
            $"auth.scope.uid={scope_uid}".WithCacheKeyPrefix();

        public static string AuthScopeAllKey() =>
            "auth.scope.all".WithCacheKeyPrefix();

        public static string AuthUserInfoKey(string user_uid) =>
            $"auth.user.uid={user_uid}".WithCacheKeyPrefix();

        public static string AuthUserRoleKey(string user_uid) =>
            $"auth.user.role={user_uid}".WithCacheKeyPrefix();

        public static string AuthUserPermissionKey(string user_uid) =>
            $"auth.user.permission={user_uid}".WithCacheKeyPrefix();

        public static string AuthSSOUserInfoKey(string user_uid) =>
            $"auth.sso.user.uid={user_uid}".WithCacheKeyPrefix();

        public static string SysPageKey(string page_name) =>
            $"sys.page.name={page_name}".WithCacheKeyPrefix();

        public static string SysCategoryListKey(string category_type) =>
            $"sys.category.list.type={category_type}".WithCacheKeyPrefix();

        public static string SysLinkListKey(string link_type) =>
            $"sys.link.list.type={link_type}".WithCacheKeyPrefix();

        public static string SysOptionListKey() =>
            $"sys.option.list.all".WithCacheKeyPrefix();

        public static string AuthStaticsReqLogGroupByHours() =>
            "auth.statics.reqlog_groupbyhour".WithCacheKeyPrefix();

        public static string AuthStaticsReqLogGroupByDate() =>
            "auth.statics.reqlog_groupbydate".WithCacheKeyPrefix();

        public static string AuthStaticsReqLogGroupByAction() =>
            "auth.statics.reqlog_groupbyaction".WithCacheKeyPrefix();

        public static string AuthStaticsCacheHitGroupByTime() =>
            "auth.statics.cachehit_groupbytime".WithCacheKeyPrefix();

        public static string AuthStaticsCacheHitGroupByKeys() =>
            "auth.statics.cachehit_groupbykeys".WithCacheKeyPrefix();

    }
}
