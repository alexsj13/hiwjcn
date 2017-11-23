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
    [Table("sys_tag_map")]
    public class TagMapModel : BaseEntity
    {
        [Column("map_key")]
        [MaxLength(100)]
        public virtual string MapKey { get; set; }

        [Column("tag_name")]
        [MaxLength(50)]
        public virtual string TagName { get; set; }

        [Column("map_type")]
        [MaxLength(20)]
        public virtual string MapType { get; set; }
    }
}
