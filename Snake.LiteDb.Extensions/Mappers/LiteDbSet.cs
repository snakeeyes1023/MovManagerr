using ChangeTracking;
using LiteDB;
using Newtonsoft.Json;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
                entity = entity.AsTrackable(ChangeStatus.Added);
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
            if (entity is not IChangeTrackable<T>)
            {
                entity = entity.AsTrackable(ChangeStatus.Changed);
            }
            if (!TrackedEntities.Any(x => x._id == entity._id))
            {
                TrackedEntities.Add(entity);
            }
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(T entity)
        {
            TrackedEntities.Remove(entity);
        }

        public void FlushData()
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection(db);
                col.DeleteAll();
            }
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

            if (changes != null)
            {
                using (var db = new LiteDatabase(ConnectionStrings))
                {
                    var col = GetCollection(db);

                    foreach (IChangeTrackable<T> item in TrackedEntities)
                    {
                        col.Insert(GetOriginal(item));
                    }

                    foreach (IChangeTrackable<T> item in changes.ChangedItems)
                    {
                        col.Update(GetOriginal(item));
                    }

                    foreach (IChangeTrackable<T> item in changes.DeletedItems)
                    {
                        col.Delete(GetOriginal(item)._id);
                    }

                }

            }

            ChangeTracker?.AcceptChanges();
        }

        private static T GetOriginal(IChangeTrackable<T> trackedEntity)
        {
            if (trackedEntity is T entity)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                
                var json = JsonConvert.SerializeObject((T)entity, settings);
                T copy = JsonConvert.DeserializeObject<T>(json, settings);
                copy._id = new ObjectId((trackedEntity as T)!._id);
                return copy;
            }

            throw new ArgumentException("");
        }

        public static ILiteCollection<T> GetCollection(LiteDatabase liteDatabase)
        {
            //get attribute Table
            var table = (typeof(T)
                .GetCustomAttributes(typeof(TableAttribute), true)
                .FirstOrDefault() as TableAttribute)?.Name;

            if (string.IsNullOrEmpty(table))
            {
                throw new Exception("Table attribute is missing");
            }

            return liteDatabase.GetCollection<T>(table);
        }
    }
}