using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Loggers;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public class MovieReader : M3uContentReader<Movie>
    {
        public MovieReader()
        {
            OnContentSynced += SearchOnTmdb;
        }

        protected override Movie? BindDataInContent(MediaM3u mediaInfo)
        {
            var movie = new Movie(mediaInfo);

            return movie;
        }

        protected override Expression<Func<MediaM3u, bool>> Filter()
        {
            return m => m.MuUrl.Contains("movie");
        }

        private void SearchOnTmdb(IEnumerable<Movie> movies)
        {
            SimpleLogger.AddLog("Recherche des films sur TMDB...", LogType.Info);


            using (var db = new LiteDatabase(Preferences._DbPath))
            {

                ILiteCollection<Movie> collection = DatabaseHelper.GetCollection<Movie>(db);

                foreach (var movie in movies)
                {
                    try
                    {
                        movie.SearchMovie();

                        collection.Update(movie);
                    }
                    catch (Exception)
                    {
                        SimpleLogger.AddLog("Le film " + movie.Name + " n'a pas été trouvé sur TMDB.", LogType.Error);
                    }
                }
            }
        }
    }
}