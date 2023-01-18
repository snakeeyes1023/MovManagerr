using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Services.Bases.ContentService;
using MovManagerr.Core.Tasks.Backgrounds;
using MovManagerr.Core.Tasks.Backgrounds.ContentTasks;
using MovManagerr.Core.Tasks.Backgrounds.MovieTasks;
using System.Linq.Expressions;

namespace MovManagerr.Core.Services.Movies
{
    public class MovieService : BaseContentService<Movie>, IMovieService
    {
        private readonly IServiceProvider _serviceProvider;

        public MovieService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Expression<Func<Movie, bool>> SearchQueryFilter(SearchQuery searchQuery)
        {
            // add tmdb id filter to base.searchqueryfilter
            Expression<Func<Movie, bool>> onTmdb = m => m.TmdbId == searchQuery.Skip;

            var baseFilter = base.SearchQueryFilter(searchQuery);
            var combinedFilter = Expression.Lambda<Func<Movie, bool>>(
                Expression.AndAlso(baseFilter.Body, onTmdb.Body),
                baseFilter.Parameters);
            return combinedFilter;
        }

        protected override IEnumerable<Movie> BaseOrderQuery(IEnumerable<Movie> results)
        {
            return results
                .Where(x => x.IsSearchedOnTmdb())
                .OrderByDescending(x => x.TmdbMovie?.ReleaseDate);
        }


        public IEnumerable<Movie> GetRecent(int limit)
        {
            (ILiteCollection<Movie> collection, LiteDatabase db) = GetDataAccess();
            
            var results = BaseOrderQuery(collection.FindAll()).Take(limit);

            db.Dispose();

            return results;
        }

        public EventedBackgroundService GetSearchAllMovieOnTmdbService()
        {
            var service = (EventedBackgroundService?)_serviceProvider.GetService(typeof(SearchAllMoviesOnTmdb));

            if (service == null)
            {
                throw new InvalidCastException("Impossible de trouver la tâche");
            }

            return service;
        }

        public EventedBackgroundService GetSyncM3UFilesInDbService()
        {
            var service = (EventedBackgroundService?)_serviceProvider.GetService(typeof(SyncM3UFiles));

            if (service == null)
            {
                throw new InvalidCastException("Impossible de trouver la tâche");
            }

            return service;
        }

        public Movie GetMovieById(ObjectId _id)
        {
            (ILiteCollection<Movie> collection, LiteDatabase db) = GetDataAccess();

            Movie movie = collection.FindById(_id);

            db.Dispose();

            return movie;
        }
    }
}
