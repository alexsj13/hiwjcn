using Hiwjcn.Dal;
using Lib.data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using System.Linq;
using Lib.extension;
using Lib.mvc.user;
using System.Text;
using System.Threading.Tasks;
using Lib.data.ef;

namespace Hiwjcn.Core.Data
{
    public class QPLSSOSqlServerConfiguration : DbConfiguration
    {
        public QPLSSOSqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }

    [DbConfigurationType(typeof(QPLSSOSqlServerConfiguration))]
    public class SSODB : BaseEFContext
    {
        public SSODB()
            : base(ConfigurationManager.ConnectionStrings["db_sso"]?.ConnectionString
                  ?? throw new Exception("请配置SSO数据库链接字符串"))
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;

            this.Database.CommandTimeout = 3;
            this.Database.Log = (log) =>
            {
                //if (!ValidateHelper.IsPlumpString(log)) { return; }
                //LogHelper.Error(typeof(EntityDB), "EF日志：\n" + log);
            };
        }

        public virtual DbSet<Auth_Department> Auth_Department { get; set; }
        public virtual DbSet<Auth_Menu> Auth_Menu { get; set; }
        public virtual DbSet<Auth_Permission> Auth_Permission { get; set; }
        public virtual DbSet<Auth_PermissionMap> Auth_PermissionMap { get; set; }
        public virtual DbSet<Auth_Role> Auth_Role { get; set; }
        public virtual DbSet<Auth_UserDepartment> Auth_UserDepartment { get; set; }
        public virtual DbSet<Auth_UserRole> Auth_UserRole { get; set; }
        public virtual DbSet<T_Systems> T_Systems { get; set; }
        public virtual DbSet<T_UserInfo> T_UserInfo { get; set; }
        public virtual DbSet<T_UserToken> T_UserToken { get; set; }
    }

    public class Auth_Department : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentID { get; set; }
        [Required(ErrorMessage = "部门名称不能为空")]
        public string DepartmentName { get; set; }
        public int ParentID { get; set; }
        public int Level { get; set; }
        public string Roles { get; set; }
    }
    public class Auth_Menu : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuID { get; set; }
        [Required(ErrorMessage = "菜单名不能为空")]
        public string MenuName { get; set; }
        public string Url { get; set; }
        public string ParentMenuID { get; set; }
        public string Permissions { get; set; }
        public int? Order { get; set; }
        public int Level { get; set; }
        public string FlagName { get; set; }
        public int SysID { get; set; }
        public string IsHidden { get; set; }
    }
    public class Auth_Permission : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PID { get; set; }
        [Required(ErrorMessage = "权限ID不能为空")]
        public string PermissionID { get; set; }
        public string ParentPermissionID { get; set; }
        [Required(ErrorMessage = "权限名称不能为空")]
        public string PermissionName { get; set; }
        public string Pdescription { get; set; }
        public string PusedMethod { get; set; }
        public int SysID { get; set; }
        public int Level { get; set; }
        public string IsRemove { get; set; }
    }
    public class Auth_PermissionMap : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MapID { get; set; }
        [Required(ErrorMessage = "权限关联key不能为空")]
        public string MapKey { get; set; }
        [Required(ErrorMessage = "关联的权限不能为空")]
        public string PermissionID { get; set; }
    }
    public class Auth_Role : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }
        [Required(ErrorMessage = "角色名称不能为空")]
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
    }
    public class Auth_UserDepartment : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MapID { get; set; }
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserID { get; set; }
        [Obsolete("已经废弃，不要使用")]
        [NotMapped]
        public string DepartmentID { get { return DepartmentID_INT.ToString(); } }
        [Required(ErrorMessage = "部门ID为空")]
        public int DepartmentID_INT { get; set; }
    }
    public class Auth_UserRole : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleID { get; set; }
        [Required(ErrorMessage = "用户ID为空")]
        public string UserID { get; set; }
        [Required(ErrorMessage = "角色ID为空")]
        public string RoleID { get; set; }
    }
    public class T_Systems : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SysID { get; set; }
        [Required(ErrorMessage = "系统名称为空")]
        public string SysName { get; set; }
        [Required(ErrorMessage = "系统退出URL为空")]
        public string LogoutUrl { get; set; }
        [Required(ErrorMessage = "系统默认地址为空")]
        public string DefaultUrl { get; set; }
        [Required(ErrorMessage = "系统图片URL为空")]
        public string ImageUrl { get; set; }
        public int IsActive { get; set; }
        public string web_token { get; set; }
        public string callback_url { get; set; }
    }
    public class T_UserInfo : IDBTable
    {
        public string CreateToken() => $"{IID}-{UID}-{PassWord}".ToMD5().ToLower();

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IID { get; set; }
        [Required(ErrorMessage = "用户UID为空")]
        public string UID { get; set; }
        [Required(ErrorMessage = "用户名为空")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "用户密码为空")]
        public string PassWord { get; set; }
        public string CompanyName { get; set; }
        public string Contact { get; set; }
        public int? Sex { get; set; }
        public string IDcard { get; set; }
        public string Images { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public int IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int IsRemove { get; set; }

        [NotMapped]
        public virtual Auth_Department Department { get; set; }

        [NotMapped]
        public virtual List<string> Permissions { get; set; }
    }
    public class T_UserToken : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IID { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
