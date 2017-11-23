using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data.mongodb
{
    [Serializable]
    public abstract class MongoEntityBase : IDBTable
    {
        public virtual ObjectId _id { get; set; }
    }
}
