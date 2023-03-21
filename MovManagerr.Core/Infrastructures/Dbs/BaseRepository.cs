using LiteDB;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Dbs
{
    
    public abstract partial class BaseRepository<T> : IBaseRepository<T> 
        where T : Entity
    {
        protected readonly ILiteDatabase DB;
        public readonly ILiteCollection<T> Collection;

        protected BaseRepository(ILiteDatabase db)
        {
            DB = db;
            Collection = db.GetCollection<T>();
        }

        public virtual T Create(T entity)
        {
            var newId = Collection.Insert(entity);
            return Collection.FindById(newId.AsInt32);
        }

        public virtual IEnumerable<T> All()
        {
            return Collection.FindAll();
        }

        public virtual T FindById(int id)
        {
            return Collection.FindById(id);
        }

        public virtual T FindOne(Expression<Func<T, bool>> predicate)
        {
            return Collection.FindOne(predicate);
        }

        public virtual void Update(T entity)
        {
            Collection.Upsert(entity);
        }

        public virtual bool Delete(int id)
        {
            return Collection.Delete(id);
        }

        public virtual ILiteQueryable<T> Query()
        {
            return Collection.Query();
        }
    }

    public interface IBaseRepository<T>
    {
        T Create(T data);
        IEnumerable<T> All();
        T FindById(int id);
        void Update(T entity);
        bool Delete(int id);
        ILiteQueryable<T> Query();
        T FindOne(Expression<Func<T, bool>> predicate);
    }
}
