using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Configuration;
using Lib.extension;

namespace Lib.infrastructure.entity
{
    public static class TokenConfig
    {
        public static readonly int TokenExpireDays = 
            (ConfigurationManager.AppSettings["AuthExpireDays"] ?? "30").ToInt(30);
        public static readonly int CodeExpireMinutes = 
            (ConfigurationManager.AppSettings["CodeExpireMinutes"] ?? "5").ToInt(5);
        public static readonly int MaxCodeCreatedDaily = 
            (ConfigurationManager.AppSettings["MaxCodeCreatedDaily"] ?? "2000").ToInt(2000);
    }

    /// <summary>
    /// auth客户端
    /// </summary>
    [Serializable]
    public class AuthClientBase : BaseEntity
    {
        [StringLength(20, MinimumLength = 1, ErrorMessage = "客户端名称必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string ClientName { get; set; }

        [MaxLength(100, ErrorMessage = "描述过长")]
        public virtual string Description { get; set; }

        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        [Url(ErrorMessage = "客户端地址必须是URL")]
        [Required]
        public virtual string ClientUrl { get; set; }

        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        [Url(ErrorMessage = "客户端图标必填")]
        [Required]
        public virtual string LogoUrl { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端用户UID必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端ClientSecretUID必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string ClientSecretUID { get; set; }

        public virtual int IsActive { get; set; }

        public virtual int IsOfficial { get; set; }
    }

    [Serializable]
    public class AuthClientCheckLogBase : BaseEntity
    {
        public virtual int CheckStatus { get; set; }

        [MaxLength(500)]
        public virtual string Msg { get; set; }
    }

    [Serializable]
    public class AuthClientUseageBase : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "client uid 名称必填")]
        [Required]
        public virtual string ClientUID { get; set; }


        [StringLength(100, MinimumLength = 20, ErrorMessage = "user uid 名称必填")]
        [Required]
        public virtual string UserUID { get; set; }
    }

    [Serializable]
    public class AuthScopeBase : BaseEntity
    {
        [StringLength(100, ErrorMessage = "scope 名称必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string Name { get; set; }

        [StringLength(100, ErrorMessage = "scope 展示名称必填")]
        [Required]
        public virtual string DisplayName { get; set; }

        [StringLength(100, ErrorMessage = "描述太长")]
        public virtual string Description { get; set; }

        public virtual int Important { get; set; }

        public virtual int Sort { get; set; }

        public virtual int IsDefault { get; set; }

        [StringLength(1000, ErrorMessage = "图片地址长度过长")]
        public virtual string ImageUrl { get; set; }

        [StringLength(30, ErrorMessage = "字体图标数据过长")]
        public virtual string FontIcon { get; set; }
    }

    [Serializable]
    public class AuthTokenBase : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "refresh token 必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string RefreshToken { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string ClientUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [DataType(DataType.Text)]
        public virtual string ScopesInfoJson { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime ExpiryTime { get; set; }

        public virtual DateTime? RefreshTime { get; set; }

        [NotMapped]
        public virtual List<AuthScopeBase> Scopes { get; set; }

        [NotMapped]
        public virtual List<string> ScopeNames { get; set; }

        [NotMapped]
        public virtual List<string> ScopeUIDS { get; set; }

        [NotMapped]
        public virtual AuthClientBase Client { get; set; }
    }

    [Serializable]
    public class AuthTokenScopeBase : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "token UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string TokenUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string ScopeUID { get; set; }
    }

    [Serializable]
    public class AuthCodeBase : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID 必填")]
        [Required(ErrorMessage = "客户端为空")]
        [Index(IsUnique = false)]
        public virtual string ClientUID { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "scopes为空")]
        public virtual string ScopesJson { get; set; }
    }
}
