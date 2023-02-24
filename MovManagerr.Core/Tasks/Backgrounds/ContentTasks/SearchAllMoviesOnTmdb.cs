using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Core.Tasks.Backgrounds.MovieTasks
{
    public class SearchAllMoviesOnTmdb : EventedBackgroundService
    {
        private readonly IContentDbContext _contentDbContext;

        private int TotalContentProceeded;
        private int TotalContentNotFounded;

        public SearchAllMoviesOnTmdb(IContentDbContext contentDbContext) : base("Rechercher les films sur Tmdb")
        {
            _contentDbContext = contentDbContext;
        }

        protected override void PerformTask(CancellationToken cancellationToken)
        {
            var getAllMovieToSearch = _contentDbContext.Movies.UseQuery(x =>
            {
                x.Where(Movie.GetIsSearchOnTmdbExpressionEnable(false));
            }).ToList();
            try

            {
                Parallel.ForEach(getAllMovieToSearch, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken }, (item) =>
                {
                    try
                    {
                        item.SearchMovieOnTmdb();

                        if (item.TmdbMovie != null)
                        {
                            Interlocked.Increment(ref TotalContentProceeded);
                        }
                        else
                        {
                            Interlocked.Increment(ref TotalContentNotFounded);
                        }

                        item.SetDirty();
                    }
                    catch (Exception)
                    {
                        SimpleLogger.AddLog("Le film " + item.Name + " a levé une exception lors de la recherche sur TMDB.", LogType.Error);
                    }
                });

                _contentDbContext.Movies.SaveChanges();
            }
            catch (OperationCanceledException)
            {
                //ok
                _contentDbContext.Movies.SaveChanges();
            }
        }

        protected override string GetEndedMessage()
        {
            var defaultMessage = base.GetEndedMessage();

            defaultMessage += $"</br> (<b>{TotalContentProceeded} contenue ont été traité. {TotalContentNotFounded} films non trouvé.)</b>";

            return defaultMessage;
        }

        protected override void ClearContext()
        {
            base.ClearContext();

            TotalContentProceeded = 0;
            TotalContentNotFounded = 0;
            
            _contentDbContext.Movies.ClearContext();
        }
    }
}
