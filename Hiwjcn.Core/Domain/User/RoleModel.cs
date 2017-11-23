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
    /// <summary>
    /// 角色
    /// </summary>
    [Table("account_role")]
    public class RoleModel : RoleEntityBase { }
}
