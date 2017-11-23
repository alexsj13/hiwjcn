using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.infrastructure.entity;

namespace Model.Sys
{
    [Table("sys_msg")]
    public class MessageModel : BaseEntity
    {
        [Column("msg_title")]
        [MaxLength(200)]
        public virtual string Title { get; set; }

        [Column("msg_content")]
        [DataType(DataType.Text)]
        public virtual string MsgContent { get; set; }
        
        [Column("msg_sender")]
        [MaxLength(100)]
        [Required]
        public virtual string SenderUserID { get; set; }

        [Column("msg_receiver")]
        [MaxLength(100)]
        [Required]
        public virtual string ReceiverUserID { get; set; }

        [Column("msg_new")]
        public virtual int IsNew { get; set; }
    }
}
