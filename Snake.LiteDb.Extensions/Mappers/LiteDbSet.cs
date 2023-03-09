using ChangeTracking;
using LiteDB;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Snake.LiteDb.Extensions.Mappers
{
    /// <summary>
    /// Permet de gérer les données en base de données LiteDB (https://www.litedb.org/)
    /// Les éléments mise à jour son seulement ceux avec le champ dirty à true
    /// </summary>
    /// <typeparam name="T">Une entité quelquonque</typeparam>
    /// <seealso cref="IEnumerable&lt;T&gt;" />
    public partial class LiteDbSet<T> : ILiteDbSet where T : Entity
    {
        public string ConnectionStrings { get; set; }

        private IList<T> TrackedEntities;

        private IChangeTrackableCollection<T>? ChangeTracker
        {
            get
            {
                if (TrackedEntities is IChangeTrackableCollection<T> changeTrackableCollection)
                {
                    return changeTrackableCollection;
                }

                return null;
            }
        }

        /// <summary>
        /// Adds the specified entity. And track it
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Add(T entity)
        {
            if (entity._id == null)
            {
                TrackedEntities.Add(entity);
            }
            else
            {
                throw new Exception("Entity already exists");
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Tracks the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public void UpdateEntity(T entity)
        {
            if (entity._id == null)
            {
                throw new ArgumentException("Entity must have an id");
            }

            TrackedEntities.Add(entity);
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(T entity)
        {
            TrackedEntities.Remove(entity);
        }


        #region DATABASE STEP

        /// <summary>
        /// Forces the refresh. (ALL UNSAVED CHANGE WILL BE CANCEL)
        /// </summary>
        public void ClearContext()
        {
            TrackedEntities.Clear();
        }

        #endregion

        public void SaveChanges()
        {
            var changes = ChangeTracker;

            if (changes?.ChangedItems is IEnumerable<IChangeTrackable<T>> trackedChanges && trackedChanges.Any())
            {
                using (var db = new LiteDatabase(ConnectionStrings))
                {
                    var col = db.GetCollection<T>();

                    foreach (IChangeTrackable<T> item in trackedChanges)
                    {
                        switch (item.ChangeTrackingStatus)
                        {
                            case ChangeStatus.Added:
                                col.Upsert((item as T)!);
                                break;
                            case ChangeStatus.Deleted:
                                col.Delete((item as T)!._id);
                                break;
                            case ChangeStatus.Changed:
                                col.Upsert((item as T)!);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            ChangeTracker?.AcceptChanges();
        }
    }
}