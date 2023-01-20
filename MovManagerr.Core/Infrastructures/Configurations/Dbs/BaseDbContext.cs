using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovManagerr.Core.Infrastructures.Configurations.Dbs
{
    public class BaseDbContext
    {
        public LiteDbSet<Movie> Movies { get; set; } = new LiteDbSet<Movie>();
    }

    public class LiteDbSet<T> : IEnumerable<T> where T : Entity
    {
        private bool needRefresh = true;

        private readonly List<T> _entities;
        private readonly List<T> _entitiesToDelete;
        private readonly List<T> _entitiesToAdd;

        private List<Action<ILiteQueryable<T>>> CustomQuery;

        public LiteDbSet()
        {
            _entities = new List<T>();
            _entitiesToDelete = new List<T>();
            _entitiesToAdd = new List<T>();
        }

        public void Add(T entity)
        {
            _entitiesToAdd.Add(entity);
        }

        public LiteDbSet<T> UseQuery(Action<ILiteQueryable<T>> query)
        {
            if (CustomQuery == null)
            {
                CustomQuery = new List<Action<ILiteQueryable<T>>>();
            }
            CustomQuery.Add(query);
            return this;
        }


        public void AddRange(IEnumerable<T> entities)
        {
            _entitiesToAdd.AddRange(entities);
        }

        public void Remove(T entity)
        {
            _entitiesToDelete.Add(entity);
            _entities.Remove(entity);
        }

        public void SaveChanges()
        {
            using (var db = new LiteDatabase(Preferences.Instance._DbPath))
            {
                var col = DatabaseHelper.GetCollection<T>(db);
                col.InsertBulk(_entitiesToAdd);
                col.DeleteMany(x => _entitiesToDelete.Any(ed => ed._id == x._id));

                foreach (var item in _entities)
                {
                    if (item.IsDirty)
                    {
                        col.Update(item);
                    }
                }
            }

            _entitiesToAdd.Clear();
            _entitiesToDelete.Clear();

            needRefresh = true;
        }


        private IEnumerable<T> GetCombinedDatasource()
        {
            if (needRefresh)
            {
                using (var db = new LiteDatabase(Preferences.Instance._DbPath))
                {
                    var col = DatabaseHelper.GetCollection<T>(db);

                    _entities.Clear();

                    if (CustomQuery != null)
                    {
                        var query = col.Query();


                        foreach (var item in CustomQuery)
                        {
                            item(query);
                        }

                        _entities.AddRange(query.ToEnumerable());
                    }
                    else
                    {
                        _entities.AddRange(col.FindAll());
                    }
                }

                needRefresh = false;

                return _entities;
            }
            else
            {
                var entities = _entities.Concat(_entitiesToAdd);

                return entities;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetCombinedDatasource().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)GetCombinedDatasource()).GetEnumerator();
        }
    }
}
