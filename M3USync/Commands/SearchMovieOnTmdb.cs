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

namespace M3USync.Commands
{
    public class SearchMovieOnTmdb : Command
    {
        public SearchMovieOnTmdb() : base("Recherche les films sur TMDB", true, true)
        {
        }

        protected override void Start()
        {
            //foreach (var movie in GetUnSearched())
            //{
            //    using (var db = new LiteDatabase(Preferences.Instance._DbPath))
            //    {
            //        try
            //        {
            //            ILiteCollection<Movie> collection = DatabaseHelper.GetCollection<Movie>(db);

            //            movie.SearchMovie();

            //            collection.Update(movie);
            //        }
            //        catch (Exception)
            //        {
            //            AwesomeConsole.WriteWarning("Le film " + movie.Name + " n'a pas été trouvé sur TMDB.");
            //        }
            //    }
            //}
        }

        public IEnumerable<Movie> GetUnSearched()
        {
            var db = new LiteDatabase(Preferences.Instance._DbPath);

            ILiteCollection<Movie> collection = DatabaseHelper.GetCollection<Movie>(db);

            var alls = collection.FindAll().ToList();

            db.Dispose();

            foreach (var movie in alls)
            {
                if (string.IsNullOrEmpty(movie.TMDBID))
                {
                    yield return movie;
                }
            }
        }
    }
}
