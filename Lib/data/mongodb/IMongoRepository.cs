using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB;
using System.Linq.Expressions;
using Lib.helper;

namespace Lib.data.mongodb
{
    public interface IMongoRepository<T> : ILinqRepository<T>, IRepository<T>
        where T : MongoEntityBase
    {
        void PrepareMongoCollection(Action<IMongoCollection<T>> callback);

        Task PrepareMongoCollectionAsync(Func<IMongoCollection<T>, Task> callback);

        R PrepareMongoCollection<R>(Func<IMongoCollection<T>, R> callback);

        Task<R> PrepareMongoCollectionAsync<R>(Func<IMongoCollection<T>, Task<R>> callback);

        List<T> QueryNearBy(Expression<Func<T, bool>> where, int page, int pagesize,
            Expression<Func<T, object>> field, GeoInfo point,
            double? max_distance = null, double? min_distance = null);
    }
}
