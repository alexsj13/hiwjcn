using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;
using Bll;
using Lib.core;
using WebLogic.Dal.User;
using Lib.helper;
using Lib.infrastructure;

namespace WebLogic.Bll.User
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    public class UserRoleBll : ServiceBase<UserRoleModel>
    {
        private UserRoleDal _UserRoleDal { get; set; }
        public UserRoleBll()
        {
            this._UserRoleDal = new UserRoleDal();
        }

        /// <summary>
        /// 添加用户角色关联
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddUserRole(UserRoleModel model)
        {
            if (!this.CheckModel(model, out var msg))
            {
                return msg;
            }

            if (_UserRoleDal.Add(model) > 0)
            {
                return this.SUCCESS;
            }
            throw new MsgException("添加用户角色关联失败");
        }

        /// <summary>
        /// 删除用户角色关联
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="role_id"></param>
        /// <returns></returns>
        public string DeleteUserRole(string user_id, string role_id)
        {
            _UserRoleDal.DeleteWhere(x => x.UserID == user_id && x.RoleID == role_id);
            return this.SUCCESS;
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public List<string> GetRolesByUserID(string uid)
        {
            var list = _UserRoleDal.GetList(x => x.UserID == uid).Select(x => x.UID).Distinct().ToList();
            return list;
        }

    }
}
