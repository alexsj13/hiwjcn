using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public class AreaEntityBase : BaseEntity
    {
        public virtual string CountryUID { get; set; }

        public virtual string ProvinceUID { get; set; }

        public virtual string CityUID { get; set; }

        public virtual string TownUID { get; set; }

        public virtual string StreetUID { get; set; }

        public virtual string Address { get; set; }

        public virtual string ContactPhone { get; set; }

        public virtual string PostCode { get; set; }
    }
}
