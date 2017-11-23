using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.extension;
using Lib.infrastructure.entity;

namespace Model.Category
{
    /// <summary>
    /// 树结构
    /// </summary>
    [Table("sys_category")]
    public class CategoryModel : BaseEntity
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        [Column("category_name")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "节点名长度错误")]
        public virtual string CategoryName { get; set; }

        /// <summary>
        /// 节点描述
        /// </summary>
        [Column("category_description")]
        [MaxLength(200, ErrorMessage = "描述文字过长")]
        public virtual string CategoryDescription { get; set; }

        /// <summary>
        /// 节点图片
        /// </summary>
        [Column("category_image")]
        [MaxLength(1000, ErrorMessage = "节点图标过长")]
        public virtual string CategoryImage { get; set; }

        /// <summary>
        /// 节点的背景图
        /// </summary>
        [Column("background_image")]
        [MaxLength(1000, ErrorMessage = "节点背景图片url过长")]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 节点颜色
        /// </summary>
        [Column("category_color")]
        [MaxLength(20, ErrorMessage = "节点颜色过长")]
        public virtual string CategoryColor { get; set; }

        /// <summary>
        /// 节点图标类
        /// </summary>
        [Column("icon_class")]
        [MaxLength(20)]
        public virtual string IconClass { get; set; }

        /// <summary>
        /// 节点URL
        /// </summary>
        [Column("link_url")]
        [MaxLength(1000)]
        public virtual string LinkURL { get; set; }

        /// <summary>
        /// 节点是否新窗口打开
        /// </summary>
        [Column("open_in_new_window")]
        public virtual int OpenInNewWindow { get; set; }

        /// <summary>
        /// 是否新窗体打开
        /// </summary>
        /// <returns></returns>
        public virtual string OpenTarget()
        {
            return OpenInNewWindow.ToBool() ? "_blank" : "_self";
        }

        /// <summary>
        /// 节点排序
        /// </summary>
        [Column("order_num")]
        public virtual int OrderNum { get; set; }

        /// <summary>
        /// 节点父级
        /// </summary>
        [Column("category_parent")]
        [StringLength(100, MinimumLength = 20)]
        public virtual string CategoryParent { get; set; }

        /// <summary>
        /// 节点层级
        /// </summary>
        [Column("category_level")]
        public virtual int CategoryLevel { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        [Column("category_type")]
        [MaxLength(20)]
        [Required(ErrorMessage = "节点类型不能为空")]
        public virtual string CategoryType { get; set; }
    }
}
