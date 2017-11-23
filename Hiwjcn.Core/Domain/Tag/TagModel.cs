using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLogic.Model;
using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Lib.infrastructure.entity;

namespace WebLogic.Model.Tag
{
    /// <summary>
    /// 标签
    /// </summary>
    [Table("sys_tag")]
    public class TagModel : BaseEntity
    {
        /// <summary>
        /// 标签名
        /// </summary>
        [Column("tag_name")]
        [MaxLength(20)]
        public virtual string TagName { get; set; }

        /// <summary>
        /// 标签描述
        /// </summary>
        [Column("tag_desc")]
        [MaxLength(500)]
        public virtual string TagDesc { get; set; }

        /// <summary>
        /// 标签外链
        /// </summary>
        [Column("tag_link")]
        [MaxLength(1000)]
        public virtual string TagLink { get; set; }

        /// <summary>
        /// 标签引用数量
        /// </summary>
        [Column("item_count")]
        public virtual int ItemCount { get; set; }

    }
}
