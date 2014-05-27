using System;
namespace Tarea.Datos
{
    public interface IRepository<TEntity>
     where TEntity : new()
    {
        int Delete(int primaryKey);
        int Delete(TEntity entity);
        int Execute(string sql, params object[] args);
        System.Collections.Generic.List<TEntity> Fetch();
        System.Collections.Generic.List<TEntity> Fetch(long page, long itemsPerPage, string sql, params object[] args);
        System.Collections.Generic.List<TEntity> Fetch(string sql, params object[] args);
        TEntity FirstOrDefault(NPoco.Sql sql);
        TEntity FirstOrDefault(string sql, params object[] args);
        object Insert(TEntity entity);
        bool IsNew(TEntity entity);
        System.Collections.Generic.IEnumerable<TEntity> Query(string sql, params object[] args);
        TEntity SingleOrDefault(NPoco.Sql sql);
        TEntity SingleOrDefault(object primaryKey);
        TEntity SingleOrDefault(string sql, params object[] args);
        System.Collections.Generic.List<TEntity> SkipTake(long skip, long take, string sql, params object[] args);
        int Update(TEntity entity);
    }
}
