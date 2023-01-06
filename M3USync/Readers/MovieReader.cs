using Hangfire;
using M3USync.Config;
using M3USync.Data;
using M3USync.Http.Models;
using M3USync.Models;
using M3USync.UIs;
using MongoDB.Driver;
using MovManagerr.Tmdb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Readers
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

            
            var collection = GetCollections();
            foreach (var movie in movies)
            {
                try
                {
                    movie.SearchMovie();

                    var filter = Builders<Movie>.Filter.Eq(s => s._id, movie._id);
                    var result = collection.ReplaceOne(filter, movie);
                }
                catch (Exception)
                {
                    AwesomeConsole.WriteWarning("Le film " + movie.Name + " n'a pas été trouvé sur TMDB.");
                }
            }
        }
    }
}
