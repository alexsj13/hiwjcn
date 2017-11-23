using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Lib.extension;

namespace Lib.data.mongodb
{
    public abstract class MongoContextBase : IDisposable
    {
        protected readonly IMongoClient _client;
        protected readonly IMongoDatabase _database;

        public MongoContextBase(string connectionString, string database)
        {
            this._client = new MongoClient(connectionString);
            this._database = this._client.GetDatabase(database);
        }

        public virtual IMongoCollection<T> Set<T>() where T : MongoEntityBase
        {
            return this._database.GetCollection<T>(typeof(T).GetTableName());
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
