using Hangfire;
using M3USync.Config;
using M3USync.Data;
using M3USync.Http;
using M3USync.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M3USync.Program;

namespace M3USync.Commands
{
    public class MovieSearcherCommand : SearcherCommand<Movie>
    {
        public MovieSearcherCommand() : base("film")
        {
        }

        protected override IEnumerable<Movie> GetCandidate(string query, IEnumerable<Movie> contentsInDb)
        {
            var tmdb = Preferences.GetTmdbInstance();
            
            var perfectMatchs = tmdb.GetRelatedMovies(query).Result;

            foreach (var item in contentsInDb)
            {
                if (perfectMatchs.Any(x => x?.Id.ToString() == item.TMDBID))
                {
                    yield return item;
                }
            }
        }
    }
}
