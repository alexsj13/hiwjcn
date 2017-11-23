using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.infrastructure.entity;

namespace WebLogic.Model.User
{
    [Serializable]
    [Table("account_login_log")]
    public class LoginErrorLogModel : BaseEntity
    {
        [MaxLength(200)]
        public virtual string LoginKey { get; set; }

        [MaxLength(200)]
        public virtual string LoginPwd { get; set; }

        [MaxLength(50)]
        public virtual string LoginIP { get; set; }

        [MaxLength(500)]
        public virtual string ErrorMsg { get; set; }
    }
}
