using Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;

namespace WebLogic.Model.User
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    [Table("account_user_role")]
    public class UserRoleModel : UserRoleEntityBase { }
}
