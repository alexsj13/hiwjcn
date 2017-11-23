using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    /// <summary>
    /// fluent map base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EFMappingBase<T> : EntityTypeConfiguration<T> where T : class, IDBTable
    {
        //
    }
}
