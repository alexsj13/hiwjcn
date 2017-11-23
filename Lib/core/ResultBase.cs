using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    public abstract class ResultBase<T>
    {
        public virtual string Msg { get; set; }
        public virtual T Data { get; set; }
    }

    public static class ResultBaseExtension
    {

    }
}
