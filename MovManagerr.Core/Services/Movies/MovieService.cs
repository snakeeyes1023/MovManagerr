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
            return base.SearchQueryFilter(searchQuery);
        }

        public IEnumerable<Movie> GetRecent(int limit)
        {
            (ILiteCollection<Movie> collection, LiteDatabase db) = GetDataAccess();

            var results = collection
                .Find(x => !string.IsNullOrWhiteSpace(x.TMDBID))
                .OrderByDescending(x => x.TmdbMovie?.ReleaseDate)
                .Take(limit);

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
    }
}
