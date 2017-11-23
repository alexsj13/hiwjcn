using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.infrastructure.entity;

namespace Hiwjcn.Core.Model.Sys
{
    [Table("sys_upfile")]
    public class UpFileModel : BaseEntity
    {
        [Column("UserID")]
        [MaxLength(100)]
        public virtual string UserID { get; set; }

        [Column("FileName")]
        [MaxLength(500)]
        public virtual string FileName { get; set; }

        [Column("FileSize")]
        public virtual int FileSize { get; set; }

        [Column("FileExt")]
        [MaxLength(20)]
        public virtual string FileExt { get; set; }

        [Column("FilePath")]
        [MaxLength(1000)]
        public virtual string FilePath { get; set; }

        [Column("FileUrl")]
        [MaxLength(1000)]
        public virtual string FileUrl { get; set; }

        [Column("FileMD5")]
        [MaxLength(100)]
        public virtual string FileMD5 { get; set; }
    }
}
