using Model;
using Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;

namespace Hiwjcn.Core.Model.Sys
{
    /// <summary>
    /// 评论系统
    /// </summary>
    [Table("sys_comment")]
    public class CommentModel : BaseEntity
    {
        /// <summary>
        /// 评论内容
        /// </summary>
        [Column("comment_content")]
        [DataType(DataType.Text)]
        public virtual string CommentContent { get; set; }

        /// <summary>
        /// 评论人
        /// </summary>
        [Column("user_id")]
        [MaxLength(100)]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 评论人
        /// </summary>
        [NotMapped]
        public virtual UserModel UserModel { get; set; }

        /// <summary>
        /// 评论对象ID
        /// </summary>
        [Column("thread_id")]
        [MaxLength(100)]
        public virtual string ThreadID { get; set; }

        /// <summary>
        /// 回复对象id
        /// </summary>
        [Column("parent_comment_id")]
        [MaxLength(100)]
        public virtual string ParentCommentID { get; set; }

        /// <summary>
        /// 父级回复
        /// </summary>
        [NotMapped]
        public virtual CommentModel ParentComment { get; set; }
    }

}
