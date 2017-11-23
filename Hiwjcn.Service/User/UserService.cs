using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.infrastructure;
using Lib.infrastructure.service;
using Model.User;
using WebLogic.Model.User;
using Lib.data;
using Lib.cache;
using Lib.ioc;
using Lib.mvc.user;
using Lib.data.ef;

namespace Hiwjcn.Bll.User
{
    public interface IUserService :
        IUserServiceBase<UserModel, UserAvatar, UserOneTimeCode, RoleModel, PermissionModel, RolePermissionModel, UserRoleModel>,
        IAutoRegistered
    { }

    [SingleInstance]
    public class UserService :
        UserServiceBase<UserModel, UserAvatar, UserOneTimeCode, RoleModel, PermissionModel, RolePermissionModel, UserRoleModel>,
        IUserService
    {
        private readonly ICacheProvider _cache;

        public UserService(
            ICacheProvider _cache,

            IEFRepository<UserModel> _userRepo,
            IEFRepository<UserAvatar> _userAvatarRepo,
            IEFRepository<UserOneTimeCode> _oneTimeCodeRepo,
            IEFRepository<RoleModel> _roleRepo,
            IEFRepository<PermissionModel> _permissionRepo,
            IEFRepository<RolePermissionModel> _rolePermissionRepo,
            IEFRepository<UserRoleModel> _userRoleRepo) :
            base(_userRepo, _userAvatarRepo, _oneTimeCodeRepo, _roleRepo, _permissionRepo, _rolePermissionRepo, _userRoleRepo)
        {
            this._cache = _cache;
        }

        public override string EncryptPassword(string password)
        {
            throw new NotImplementedException();
        }

        public override LoginUserInfo ParseUser(UserModel model)
        {
            throw new NotImplementedException();
        }

        public override void UpdatePermissionEntity(ref PermissionModel old_permission, ref PermissionModel new_permission)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRoleEntity(ref RoleModel old_role, ref RoleModel new_role)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUserEntity(ref UserModel old_user, ref UserModel new_user)
        {
            throw new NotImplementedException();
        }
    }
}
