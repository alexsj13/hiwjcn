using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.infrastructure.entity;

namespace Model.Sys
{
    [Table("sys_links")]
    public class LinkModel : BaseEntity
    {
        /// <summary>
        /// 链接名称
        /// </summary>
        [Column("link_name")]
        [MaxLength(50)]
        [Required]
        public virtual string Name { get; set; }

        /// <summary>
        /// 链接url
        /// </summary>
        [Column("link_url")]
        [MaxLength(1000)]
        public virtual string Url { get; set; }

        /// <summary>
        /// 链接标题
        /// </summary>
        [Column("link_title")]
        [MaxLength(100)]
        public virtual string Title { get; set; }

        /// <summary>
        /// 链接图片
        /// </summary>
        [Column("link_image")]
        [MaxLength(1000)]
        public virtual string Image { get; set; }

        /// <summary>
        /// 目标（self blank）
        /// </summary>
        [Column("link_target")]
        [MaxLength(10)]
        public virtual string Target { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        [Column("order_num")]
        public virtual int OrderNum { get; set; }

        /// <summary>
        /// 所有人
        /// </summary>
        [Column("link_owner")]
        [MaxLength(100)]
        [Required]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 链接的类型，比如友情链接、轮播图、合作伙伴
        /// </summary>
        [Column("link_type")]
        [MaxLength(20)]
        public virtual string LinkType { get; set; }

    }
}
