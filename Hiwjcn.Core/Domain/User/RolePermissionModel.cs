using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;

namespace WebLogic.Model.User
{
    [Table("account_permission")]
    public class PermissionModel : PermissionEntityBase { }

    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Table("account_role_permission")]
    public class RolePermissionModel : RolePermissionEntityBase { }
}
