using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 统一不同数据库之间的数据集合
    /// 
    /// //这样ef的多次add一次提交就没用了
    /// 
    /// 建一个base repository
    /// ef的repository继承base repository
    /// 
    /// </summary>
    public interface IDataCollection<T>
    {
        int Add();
        int Delete();
        int Update();
        int Query();
    }

    public interface IDataSourceContext
    {
        IDataCollection<T> Set<T>() where T : class;
    }

    public class EfDataSourceContext : IDataSourceContext
    {
        public IDataCollection<T> Set<T>() where T : class
        {
            return new EfDataCollection<T>(() => null);
        }
    }

    public class EfDataCollection<T> : IDataCollection<T> where T : class
    {
        private readonly Func<DbContext> GetContext;

        public EfDataCollection(Func<DbContext> GetContext)
        {
            this.GetContext = GetContext ?? throw new Exception();
        }

        public int Add()
        {
            using (var db = GetContext())
            {
                db.Set<T>().Add(null);
                return db.SaveChanges();
            }
        }

        public int Delete()
        {
            throw new NotImplementedException();
        }

        public int Query()
        {
            throw new NotImplementedException();
        }

        public int Update()
        {
            throw new NotImplementedException();
        }
    }

    public class MongoDataCollection<T> : IDataCollection<T>
    {
        public int Add()
        {
            throw new NotImplementedException();
        }

        public int Delete()
        {
            throw new NotImplementedException();
        }

        public int Query()
        {
            throw new NotImplementedException();
        }

        public int Update()
        {
            throw new NotImplementedException();
        }
    }
}
