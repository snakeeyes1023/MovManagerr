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
        public UseQueryFunc<T, object> ServerQuery { get; set; }


        public delegate Tout UseQueryFunc<Ti, Tout>(ILiteQueryable<Ti> query);

        /// <summary>
        /// Uses the query. (Directly bind to entity)
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public Tout UseQuery<Tout>(UseQueryFunc<T, Tout> query) where Tout : class
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = db.GetCollection<T>();

                Tout result = query(col.Query());

                if (result is IEnumerable<T> entities)
                {
                    entities.AsTrackable();

                    

                }
                else if (result is T entity)
                {
                    var trackedEntity = entity.AsTrackable();

                    TrackedEntities.Add(trackedEntity);

                    return (trackedEntity as Tout)!;
                }
            }
        }

        public IEnumerable<T> All()
        {
            UseQuery((query) =>
            {
                return query.FirstOrDefault();
            });
            return TrackedEntities;
        }


        public IEnumerable<T> ExecuteQuery()
        {
            TrackedEntities = new List<T>().AsTrackable();
            return TrackedEntities;
        }
    }
}
