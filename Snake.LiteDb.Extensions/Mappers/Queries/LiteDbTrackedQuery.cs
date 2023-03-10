using ChangeTracking;
using LiteDB;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.LiteDb.Extensions.Mappers
{
    public partial class LiteDbSet<T> : ILiteDbSet where T : Entity
    {
        public LiteDbSet()
        {
            TrackedEntities = (new List<T>()).AsTrackable();
        }


        public UseQueryFunc<T, object> ServerQuery { get; set; }


        public delegate Tout UseQueryFunc<Ti, Tout>(ILiteQueryable<Ti> query);

        /// <summary>
        /// Uses the query. (Directly bind to entity)
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public Tout UseQuery<Tout>(UseQueryFunc<T, Tout> query) where Tout : struct
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection(db);

                return query(col.Query());
            }
        }

        /// <summary>
        /// Uses the query. (Directly bind to entity)
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public T UseQuery(UseQueryFunc<T, T> query)
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection(db);

                T result = query(col.Query());

                return AddEntityToTracked(result);
            }
        }

        public IEnumerable<T> UseQuery(UseQueryFunc<T, IEnumerable<T>> query)
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection(db);

                IEnumerable<T> result = query(col.Query());

                var trackedEntities = new List<T>();

                foreach (var entity in result)
                {
                    trackedEntities.Add(AddEntityToTracked(entity));
                }

                return trackedEntities;
            }
        }

        public T AddEntityToTracked(T entity)
        {
            var trackedEntity = entity.AsTrackable();

            TrackedEntities.Add(trackedEntity);

            return trackedEntity;
        }
    }
}
