using LiteDB;
using M3USync.Data;
using M3USync.Data.Helpers;
using M3USync.Downloaders.M3U;
using M3USync.Infrastructures.UIs;
using System.Linq.Expressions;

namespace M3USync.Downloaders.Contents.Readers
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
            AwesomeConsole.WriteInfo("Recherche des films sur TMDB...");


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
                        AwesomeConsole.WriteWarning("Le film " + movie.Name + " n'a pas été trouvé sur TMDB.");
                    }
                }
            }
        }
    }
}