using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace Tarea.Datos
{
    public class Repository<TEntity> : Datos.IRepository<TEntity> where TEntity : new()
    {
        
        public Repository(IUnitOfWork uow) {
            Database = uow.bD;
        }

        public IDatabase Database;
        

        public bool IsNew(TEntity entity) { return Database.IsNew<TEntity>(entity); }

        public TEntity SingleOrDefault(object primaryKey) { return Database.SingleOrDefaultById<TEntity>(primaryKey); }
        public TEntity SingleOrDefault(string sql, params object[] args) { return Database.SingleOrDefault<TEntity>(sql, args); }
        public TEntity SingleOrDefault(Sql sql) { return Database.SingleOrDefault<TEntity>(sql); }
        public TEntity FirstOrDefault(string sql, params object[] args) { return Database.FirstOrDefault<TEntity>(sql, args); }
        public TEntity FirstOrDefault(Sql sql) { return Database.FirstOrDefault<TEntity>(sql); }

        public List<TEntity> Fetch() { return Database.Fetch<TEntity>(""); }
        public List<TEntity> Fetch(string sql, params object[] args) { return Database.Fetch<TEntity>(sql, args); }
        public List<TEntity> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return Database.Fetch<TEntity>(page, itemsPerPage, sql, args); }
        public List<TEntity> SkipTake(long skip, long take, string sql, params object[] args) { return Database.SkipTake<TEntity>(skip, take, sql, args); }
        public IEnumerable<TEntity> Query(string sql, params object[] args) { return Database.Query<TEntity>(sql, args); }
        public  int Execute(string sql, params object[] args) { return Database.Execute(sql, args); }

        public object Insert(TEntity entity)
        {
            return Database.Insert(entity);
        }

        public  int Update(TEntity entity)
        {
            return Database.Update(entity);
        }

        public  int Delete(TEntity entity)
        {
            return Database.Delete(entity);
        }

        public  int Delete(int primaryKey)
        {
            var poco = SingleOrDefault(primaryKey);
            if (poco != null)
                return Delete(poco);

            return 0;
        }
    }
}
