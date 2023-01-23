using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Dbs;
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

        public MovieService(IServiceProvider serviceProvider, IContentDbContext contentDbContext) : base(contentDbContext)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Movie> GetRecent(int limit)
        {
            return _currentCollection.UseQuery(x =>
            {
                x.Limit(limit);
                BaseOrderQuery(x);
            }).ToList();
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

        public Movie? GetMovieById(ObjectId _id)
        {
            Movie? movie = _currentCollection.UseQuery(x =>
            {
                x.Where(x => x._id == _id).FirstOrDefault();
                
            }).FirstOrDefault();

            if (movie != null && !movie.IsSearchedOnTmdb())
            {
                movie.SearchMovieOnTmdb();

                movie.SetDirty(true);

                _currentCollection.SaveChanges();
            }

            return movie;
        }
    }
}
