using Hiwjcn.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain;
using Hiwjcn.Core.Model.Sys;
using WebLogic.Model.User;
using Model.User;
using Lib.extension;
using WebLogic.Model.Tag;
using Lib.data.ef;

namespace Hiwjcn.Bll
{
    public class ClearDataBaseService : IClearDataBaseService
    {
        private readonly IEFRepository<AuthClient> _AuthClientRepo;
        private readonly IEFRepository<AuthScope> _AuthScopeRepo;
        private readonly IEFRepository<AuthToken> _AuthTokenRepo;
        private readonly IEFRepository<AuthCode> _AuthCodeRepo;
        private readonly IEFRepository<AuthTokenScope> _AuthTokenScopeRepo;

        private readonly IEFRepository<ReqLogModel> _ReqLogModelRepo;
        private readonly IEFRepository<CacheHitLog> _CacheHitLogRepo;

        private readonly IEFRepository<LoginErrorLogModel> _LoginErrorLogModelRepo;

        public ClearDataBaseService(
            IEFRepository<AuthClient> _AuthClientRepo,
            IEFRepository<AuthScope> _AuthScopeRepo,
            IEFRepository<AuthToken> _AuthTokenRepo,
            IEFRepository<AuthCode> _AuthCodeRepo,
            IEFRepository<AuthTokenScope> _AuthTokenScopeRepo,

            IEFRepository<ReqLogModel> _ReqLogModelRepo,
            IEFRepository<CacheHitLog> _CacheHitLogRepo,

            IEFRepository<LoginErrorLogModel> _LoginErrorLogModelRepo)
        {
            this._AuthClientRepo = _AuthClientRepo;
            this._AuthScopeRepo = _AuthScopeRepo;
            this._AuthTokenRepo = _AuthTokenRepo;
            this._AuthCodeRepo = _AuthCodeRepo;
            this._AuthTokenScopeRepo = _AuthTokenScopeRepo;

            this._ReqLogModelRepo = _ReqLogModelRepo;
            this._CacheHitLogRepo = _CacheHitLogRepo;

            this._LoginErrorLogModelRepo = _LoginErrorLogModelRepo;
        }

        public void ClearCacheHitLog()
        {
            var expire = DateTime.Now.AddDays(-30);
            this._CacheHitLogRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearClient()
        {
            this._AuthClientRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserModel>();
                var client_set = db.Set<AuthClient>();

                var uids = user_set.Select(x => x.UID);
                var range = client_set.Where(x => !uids.Contains(x.UserUID));
                client_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearLoginLog()
        {
            var expire = DateTime.Now.AddDays(-100);
            this._LoginErrorLogModelRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearPage()
        {
            "页面的数据不会被清理".AddBusinessInfoLog();
        }

        public void ClearPermission()
        {
            this._LoginErrorLogModelRepo.PrepareSession(db =>
            {
                var role_set = db.Set<RoleModel>();
                var permission_set = db.Set<PermissionModel>();
                var permission_map = db.Set<RolePermissionModel>();

                var role_uids = role_set.Select(x => x.UID);
                var permission_uids = permission_set.Select(x => x.UID);
                var range = permission_map.Where(x => !role_uids.Contains(x.RoleID) || !permission_uids.Contains(x.PermissionID));

                permission_map.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearRequestLog()
        {
            var expire = DateTime.Now.AddDays(-30);
            this._ReqLogModelRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearRole()
        {
            this._LoginErrorLogModelRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserModel>();
                var role_set = db.Set<RoleModel>();
                var role_map_set = db.Set<UserRoleModel>();

                var user_uids = user_set.Select(x => x.UID);
                var role_uids = role_set.Select(x => x.UID);
                var range = role_map_set.Where(x => !user_uids.Contains(x.UserID) || !role_uids.Contains(x.RoleID));
                role_map_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearScope()
        {
            this._AuthScopeRepo.PrepareSession(db =>
            {
                var token_set = db.Set<AuthToken>();
                var scope_set = db.Set<AuthTokenScope>();

                var uids = token_set.Select(x => x.UID);
                var range = scope_set.Where(x => !uids.Contains(x.TokenUID));

                scope_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearTag()
        {
            this._ReqLogModelRepo.PrepareSession(db =>
            {
                var tag_set = db.Set<TagModel>();
                var tag_map = db.Set<TagMapModel>();

                return true;
            });
        }

        public void ClearToken()
        {
            var now = DateTime.Now;
            this._AuthTokenRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserModel>();
                var token_set = db.Set<AuthToken>();
                var client_set = db.Set<AuthClient>();

                var user_uids = user_set.Select(x => x.UID);
                var client_uids = client_set.Select(x => x.UID);
                var range = token_set.Where(x => x.ExpiryTime < now || (!user_uids.Contains(x.UserUID)) || (!client_uids.Contains(x.ClientUID)));
                token_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearUser()
        {
            "用户信息不会被清理".AddBusinessInfoLog();
        }
    }
}
