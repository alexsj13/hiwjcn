using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class AddressModel
    {
        public virtual string Id { get; set; }

        public virtual string ServiceNodeName { get; set; }

        public virtual string EndpointNodeName { get; set; }

        public virtual string FullName { get => $"{this.ServiceNodeName}/{this.EndpointNodeName}"; }

        public virtual string Url { get; set; }
        
        public virtual DateTime? OnLineTime { get; set; }
    }
}
