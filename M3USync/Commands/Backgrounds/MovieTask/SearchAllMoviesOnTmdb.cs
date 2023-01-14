using LiteDB;
using M3USync.Data;
using M3USync.Data.Helpers;
using M3USync.Infrastructures.Configurations;
using M3USync.Infrastructures.UIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands.Backgrounds.MovieTask
{
    public class SearchAllMoviesOnTmdb : EventedBackgroundService
    {
        private int TotalContentProceeded;
        private int TotalContentNotFounded;

        public SearchAllMoviesOnTmdb() : base("Rechercher les films sur Tmdb")
        {
        }

        protected override void PerformTask(CancellationToken cancellationToken)
        {
            var getAllMovieToSearch = GetAllUnFoundedMovie();

            try
            {
                Parallel.ForEach(getAllMovieToSearch, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken }, (item) =>
                {
                    try
                    {
                        item.TmdbMovie = item.SearchMovie();

                        if (item.TmdbMovie != null)
                        {
                            UpdateMovie(item);
                            Interlocked.Increment(ref TotalContentProceeded);
                        }
                        else 
                        {
                            Interlocked.Increment(ref TotalContentNotFounded);
                        }
                    }
                    catch (Exception)
                    {
                        SimpleLogger.AddLog("Le film " + item.Name + " a levé une exception lors de la recherche sur TMDB.", LogType.Error);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                //ok
            }
        }

        protected override string GetEndedMessage()
        {
            var defaultMessage = base.GetEndedMessage();

            defaultMessage += $"</br> (<b>{TotalContentProceeded} contenue ont été traité. {GetAllUnFoundedMovie().Count()} restants. {TotalContentNotFounded} films non trouvé.)</b>";

            return defaultMessage;
        }

        protected override void ClearContext()
        {
            base.ClearContext();

            TotalContentProceeded = 0;
            TotalContentNotFounded = 0;
        }


        private IEnumerable<Movie> GetAllUnFoundedMovie()
        {
            var db = new LiteDatabase(Preferences.Instance._DbPath);

            ILiteCollection<Movie> collection = DatabaseHelper.GetCollection<Movie>(db);

            var recents = collection.Find(x => string.IsNullOrWhiteSpace(x.TMDBID)).ToList();

            db.Dispose();

            return recents;
        }

        private void UpdateMovie(Movie movie)
        {
            var db = new LiteDatabase(Preferences.Instance._DbPath);

            ILiteCollection<Movie> collection = DatabaseHelper.GetCollection<Movie>(db);

            collection.Update(movie);

            db.Dispose();
        }
    }
}
