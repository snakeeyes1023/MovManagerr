﻿using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Infrastructures.Configurations;
using System.Linq.Expressions;

namespace MovManagerr.Core.Services.Bases.ContentService
{
    public abstract class BaseContentService<T> : IBaseContentService<T> where T : Content
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            (ILiteCollection<T> collection, LiteDatabase db) = GetDataAccess();

            var results = collection.FindAll();

            db.Dispose();

            return results;
        }

        /// <summary>
        /// Gets the candidates for a given SearchQuery.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetCandidates(SearchQuery searchQuery)
        {
            (ILiteCollection<T> collection, LiteDatabase db) = GetDataAccess();

            var results = collection
                .Find(SearchQueryFilter(searchQuery), searchQuery.Skip, searchQuery.Take);

            db.Dispose();

            return results;
        }

        /// <summary>
        /// Searches the query filter.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        protected virtual Expression<Func<T, bool>> SearchQueryFilter(SearchQuery searchQuery)
        {
            return x => x.Name.Contains(searchQuery.EnteredText, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the data access.
        /// </summary>
        /// <returns></returns>
        protected (ILiteCollection<T>, LiteDatabase) GetDataAccess()
        {
            var db = new LiteDatabase(Preferences.Instance._DbPath);
            ILiteCollection<T> collection = DatabaseHelper.GetCollection<T>(db);
            return (collection, db);
        }
    }
}