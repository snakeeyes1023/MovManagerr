using Microsoft.Extensions.Options;
using MovManagerr.Tmdb.Config;
using MovManagerr.Tmdb.Service;
using System.Xml.Linq;
using TMDbLib.Client;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace MovManagerr.Tmdb
{
    public class TmdbClientService
    {
        private readonly TMDbClient _client;
        public readonly FavoriteService Favorites;
        private TmdbConfig tmdbConfig;

        public TmdbClientService(IOptions<Tmdb.Config.TmdbConfig> config)
        {
            _client = new TMDbClient(config.Value.ApiKey, config.Value.UseSsl, config.Value.Url);
            _client.SetSessionInformationAsync(config.Value.Session, SessionType.UserSession).Wait();
            _client.DefaultLanguage = "en";

            Favorites = new FavoriteService(this);
        }

        /// <summary>
        /// Get movie by name and year
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<Movie?> GetMovieByNameAndYearAsync(string name, int year)
        {
            var search = await _client.SearchMovieAsync(name, 0, includeAdult : false, year);
            var result = search.Results.FirstOrDefault();

            return result != null ? await _client.GetMovieAsync(result.Id) : null;
        }

        /// <summary>
        /// Get movie by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _client.GetMovieAsync(id);
        }

        public Movie? GetMovieById(int id)
        {
            Task<Movie?> task = Task.Run<Movie?>(async () => await _client.GetMovieAsync(id));
            return task.Result;
        }


        public IEnumerable<SearchMovie?>? GetRelatedMovies(string name)
        {
            Task<SearchContainer<SearchMovie>?> task = Task.Run<SearchContainer<SearchMovie>?>(async () => await _client.SearchMovieAsync(name, 0, includeAdult: false));
           return task.Result?.Results;
        }

        public TMDbClient GetClient()
        {
            return _client;
        }

    }
}