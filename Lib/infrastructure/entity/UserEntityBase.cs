using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public class UserEntityBase : BaseEntity
    {
        [Column("user_name")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "用户名长度不匹配")]
        public virtual string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column("nick_name")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "昵称长度不匹配")]
        public virtual string NickName { get; set; }

        /// <summary>
        /// md5加密的密码
        /// </summary>
        [Column("user_pass")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "密码长度错误")]
        public virtual string PassWord { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        [Column("user_phone")]
        [StringLength(50)]
        public virtual string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("user_email")]
        [StringLength(50)]
        [EmailAddress(ErrorMessage = "邮件格式错误")]
        public virtual string Email { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        [Column("user_img")]
        [StringLength(1000)]
        public virtual string UserImg { get; set; }

        /// <summary>
        /// 用户拥有的权限（大分类）
        /// </summary>
        [Column("user_flag")]
        public virtual int Flag { get; set; }

        [Column("is_active")]
        public virtual int IsActive { get; set; }

        /// <summary>
        /// 用户所有角色id
        /// </summary>
        [NotMapped]
        public virtual List<string> RoleList { get; set; }

        /// <summary>
        /// 用户所有权限id（最终拿来验证用户行为是否有权限）
        /// </summary>
        [NotMapped]
        public virtual List<string> PermissionList { get; set; }

        /// <summary>
        /// 用户角色模型
        /// </summary>
        [NotMapped]
        public virtual List<RoleEntityBase> RoleModelList { get; set; }

        [NotMapped]
        public virtual List<DepartmentEntityBase> DepartmentModelList { get; set; }

        [NotMapped]
        public virtual List<DepartmentRoleEntityBase> DepartmentRoleModelList { get; set; }
    }

    [Serializable]
    public class UserAvatarEntityBase : BaseEntity
    {
        [Required(ErrorMessage = "头像数据为空")]
        public virtual byte[] AvatarBytes { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "UserUID长度错误")]
        [Required(ErrorMessage = "UserUID必填")]
        [Index]
        public virtual string UserUID { get; set; }
    }

    [Serializable]
    public class UserOneTimeCodeEntityBase : BaseEntity
    {
        [StringLength(100)]
        [Required]
        public virtual string Code { get; set; }

        [StringLength(100)]
        [Required]
        public virtual string UserUID { get; set; }

        public virtual int CodeType { get; set; }
    }

    [Serializable]
    public class RoleEntityBase : TreeEntityBase
    {
        /// <summary>
        /// 角色名
        /// </summary>
        [Column("role_name")]
        [MaxLength(20)]
        [Required]
        public virtual string RoleName { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [Column("role_desc")]
        [MaxLength(500)]
        public virtual string RoleDescription { get; set; }

        /// <summary>
        /// 用户注册的时候自动分配这些权限
        /// </summary>
        [Column("auto_assign_to_user")]
        public virtual int AutoAssignRole { get; set; }
    }

    [Serializable]
    public class PermissionEntityBase : TreeEntityBase
    {
        [MaxLength(20)]
        [Required]
        public virtual string Name { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }
    }

    [Serializable]
    public class RolePermissionEntityBase : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [MaxLength(100)]
        [Required]
        public virtual string RoleID { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [MaxLength(100)]
        [Required]
        public virtual string PermissionID { get; set; }
    }

    [Serializable]
    public class UserRoleEntityBase : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        [StringLength(100)]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        [StringLength(100)]
        public virtual string RoleID { get; set; }
    }

    [Serializable]
    public class DepartmentEntityBase : TreeEntityBase
    {
        public virtual string DepartmentName { get; set; }

        public virtual string Description { get; set; }
    }

    [Serializable]
    public class UserDepartmentEntityBase : BaseEntity
    {
        public virtual string UserUID { get; set; }

        public virtual string DepartmentUID { get; set; }
    }

    [Serializable]
    public class DepartmentRoleEntityBase : BaseEntity
    {
        public virtual string DepartmentUID { get; set; }

        public virtual string RoleUID { get; set; }
    }
}
