using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;

namespace Hiwjcn.Core.Infrastructure.User
{
    public interface IRoleService : IServiceBase<RoleModel>
    {
        string AddRole(RoleModel model);

        /// <summary>
        /// 跟新角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string UpdateRole(RoleModel model);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string DeleteRole(string RoleID);

        /// <summary>
        /// 获取角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RoleModel GetByID(string id);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        List<RoleModel> GetAllRoles();

        /// <summary>
        /// 获取角色下的所有权限id
        /// </summary>
        /// <param name="role_id"></param>
        /// <returns></returns>
        List<string> GetRolePermissionsList(string role_id);
    }
}
