using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovManagerr.Core.Infrastructures.Configurations.Dbs
{
    public class BaseDbContext
    {
        public IQueryable MyProperty { get; set; }
    }

    public class LiteDbSet<T> : IEnumerable<T> where T : Entity
    {
        private readonly List<T> _entities = new List<T>();

        public void Add(T entity)
        {
            _entities.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _entities.AddRange(entities);
        }

        public void Remove(T entity)
        {
            _entities.Remove(entity);
        }

        public void SaveChanges()
        {
            var db = new LiteDatabase(Preferences.Instance._AppData);
            var collection = db.GetCollection<T>();
            collection.InsertBulk(_entities);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_entities).GetEnumerator();
        }
    }
}
