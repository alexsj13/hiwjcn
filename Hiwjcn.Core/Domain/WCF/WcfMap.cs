using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Model.User;
using System.Runtime.Serialization;
using Model;
using System.Configuration;
using Lib.extension;
using Lib.infrastructure.entity;

namespace Hiwjcn.Core.Domain.WCF
{
    [Serializable]
    [Table("wcf_map")]
    public class WcfMap : BaseEntity
    {
        [Required]
        public virtual string ContractName { get; set; }

        [Required]
        public virtual string SvcUrl { get; set; }

        [Required]
        public virtual DateTime HeartBeatsTime { get; set; }
    }
}
