using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using Snake.LiteDb.Extensions.Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MovManagerr.Core.Services.Bases.ContentService
{
    public abstract class BaseContentService<T> : IBaseContentService<T> where T : Content
    {
        private readonly IContentDbContext _contentDbContext;

        protected readonly LiteDbSet<T> _currentCollection;

        public BaseContentService(IContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;

            _currentCollection = _contentDbContext.GetCollection<T>();
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll(int offset, int limit)
        {
            var entities = _currentCollection.UseQuery(x =>
             {
                 x.Skip(offset);
                 x.Limit(limit);
                 BaseOrderQuery(x);
             });

            return entities.ToList();
        }

        /// <summary>
        /// Gets the candidates for a given SearchQuery.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetCandidates(SearchQuery searchQuery)
        {
            var entities = _currentCollection.UseQuery(x =>
            {
                x.Where(x => x.Name.Contains(searchQuery.EnteredText));
                x.Skip(searchQuery.Skip);
                x.Limit(searchQuery.Take);
            });

            return entities.ToList();
        }

        /// <summary>
        /// Bases the results query. Order by ID descending default
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        protected virtual void BaseOrderQuery(ILiteQueryable<T> queryable)
        {
            queryable.OrderByDescending(x => x._id);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns></returns>
        public virtual int GetCount()
        {
            return _currentCollection.ToList().Where(x => x.IsDownloaded).Count();
        }       

        public LiteDbSet<T> GetCurrentCollection()
        {
            return _currentCollection;
        }
    }
}
