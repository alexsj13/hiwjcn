using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public class CatalogEntityBase : TreeEntityBase
    {
        public virtual string CatalogName { get; set; }

        public virtual string CatalogDescription { get; set; }
    }
}
