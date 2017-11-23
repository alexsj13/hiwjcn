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
    /// 角色权限关联
    /// </summary>
    public class RolePermissionBll : ServiceBase<RolePermissionModel>
    {
        private RolePermissionDal _RolePermissionDal { get; set; }
        public RolePermissionBll()
        {
            this._RolePermissionDal = new RolePermissionDal();
        }

        /// <summary>
        /// 添加角色权限关联
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddRolePermission(RolePermissionModel model)
        {
            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }
            return _RolePermissionDal.Add(model) > 0 ? SUCCESS : "保存失败";
        }

        /// <summary>
        /// 删除权限角色关联
        /// </summary>
        /// <param name="role_id"></param>
        /// <param name="permission_id"></param>
        /// <returns></returns>
        public string DeleteRolePermission(string role_id, string permission_id)
        {
            if (!ValidateHelper.IsInt(permission_id))
            {
                return "参数错误";
            }
            var list = _RolePermissionDal.QueryList(
                x => x.RoleID == role_id && x.PermissionID == permission_id,
                x => x.CreateTime);
            if (!ValidateHelper.IsPlumpList(list))
            {
                return "要删除的数据不存在";
            }
            return _RolePermissionDal.Delete(list.ToArray()) > 0 ? SUCCESS : "删除失败";
        }

    }
}
