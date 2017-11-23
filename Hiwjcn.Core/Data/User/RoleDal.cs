using Dal;
using Lib.data;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;
using Lib.data.ef;

namespace WebLogic.Dal.User
{
    /// <summary>
    /// 角色
    /// </summary>
    public class RoleDal : EFRepository<RoleModel>
    {
        public RoleDal() { }
    }
}
