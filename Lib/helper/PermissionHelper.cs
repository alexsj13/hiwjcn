using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.helper
{
    public enum PermissionExample : int
    {
        开店 = 1 << 0,
        购物 = 1 << 1,
        发帖 = 1 << 2,
        终极管理员 = 1 << 3,
        协助管理员 = 1 << 4,
        普通用户 = 1 << 5
    }

    /// <summary>
    /// 基于位运算的权限验证，权限必须是2的次方
    /// var per = (int)(PermissionExample.协助管理员 | PermissionExample.终极管理员 | PermissionExample.普通用户);
    /// var valid = PermissionHelper.HasPermission(per, (int)PermissionExample.普通用户);
    /// valid = PermissionHelper.HasPermission(per, (int) PermissionExample.开店);
    /// PermissionHelper.AddPermission(ref per, (int) PermissionExample.开店);
    /// valid = PermissionHelper.HasPermission(per, (int) PermissionExample.开店);
    /// PermissionHelper.RemovePermission(ref per, (int) PermissionExample.终极管理员);
    /// valid = PermissionHelper.HasPermission(per, (int) PermissionExample.终极管理员);
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// 添加一个权限
        /// </summary>
        public static void AddPermission(ref int user_permission, int new_permission)
        {
            user_permission = user_permission | new_permission;
        }

        /// <summary>
        /// 删除一个权限
        /// </summary>
        public static void RemovePermission(ref int user_permission, int removed_permission)
        {
            user_permission = user_permission & ~removed_permission;
        }

        /// <summary>
        /// 是否有权限
        /// </summary>
        public static bool HasPermission(int user_permission, int single_permission_to_valid)
        {
            return (user_permission & single_permission_to_valid) == single_permission_to_valid;
        }

        /// <summary>
        /// 满足所有权限
        /// </summary>
        public static bool HasAllPermission(int user_permission, params int[] permission_to_valid)
        {
            if (!ValidateHelper.IsPlumpList(permission_to_valid)) { throw new ArgumentException(nameof(permission_to_valid)); }
            return permission_to_valid.All(x => HasPermission(user_permission, x));
        }

        /// <summary>
        /// 满足部门权限
        /// </summary>
        public static bool HasAnyPermission(int user_permission, params int[] permission_to_valid)
        {
            if (!ValidateHelper.IsPlumpList(permission_to_valid)) { throw new ArgumentException(nameof(permission_to_valid)); }
            return permission_to_valid.Any(x => HasPermission(user_permission, x));
        }
    }
}
