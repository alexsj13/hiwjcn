using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;

namespace Lib.mvc.user
{
    public interface UserModelBase : UserModelBase<int>
    { }

    public interface UserModelBase<KeyType> : IDBTable
    {
        KeyType IID { get; set; }

        string UID { get; set; }

        string UserName { get; set; }

        string Password { get; set; }

        string Email { get; set; }
    }

    public interface RoleModelBase : IDBTable
    { }

    public interface UserRoleModelBase : IDBTable
    {

    }

    public interface PermissionModelBase : IDBTable
    { }

    public interface RolePermissionModelBase : IDBTable
    { }
}
