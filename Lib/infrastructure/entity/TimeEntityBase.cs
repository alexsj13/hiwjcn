using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public abstract class TimeEntityBase : BaseEntity
    {
        public virtual int TimeYear { get; set; }

        public virtual int TimeMonth { get; set; }

        public virtual int TimeDay { get; set; }

        public virtual int TimeHour { get; set; }

        public override void Init(string flag = null)
        {
            base.Init(flag);
            this.TimeYear = this.CreateTime.Year;
            this.TimeMonth = this.CreateTime.Month;
            this.TimeDay = this.CreateTime.Day;
            this.TimeHour = this.CreateTime.Hour;
        }
    }
}
