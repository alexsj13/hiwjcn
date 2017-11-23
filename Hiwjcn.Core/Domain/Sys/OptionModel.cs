using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.infrastructure.entity;

namespace Model.Sys
{
    [Table("sys_options")]
    public class OptionModel : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Column("option_name")]
        [MaxLength(200)]
        [Required]
        public virtual string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Column("option_value")]
        [MaxLength(200)]
        public virtual string Value { get; set; }
    }
}
