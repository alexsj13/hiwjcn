using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public abstract class TreeEntityBase : BaseEntity
    {
        public const int FIRST_LEVEL = 0;
        public const string FIRST_PARENT_UID = "-1";
        public const string DEFAULT_GROUP = "default";

        [Required]
        [Range(FIRST_LEVEL, FIRST_LEVEL + 500, ErrorMessage = "层级不在范围之内")]
        public virtual int Level { get; set; } = FIRST_LEVEL;

        [Required]
        [StringLength(100, ErrorMessage = "父级UID长度错误")]
        public virtual string ParentUID { get; set; } = FIRST_PARENT_UID;

        [Required]
        [StringLength(100, ErrorMessage = "分组长度错误")]
        public virtual string GroupKey { get; set; } = DEFAULT_GROUP;
    }
}
