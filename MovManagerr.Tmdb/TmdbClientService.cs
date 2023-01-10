using Microsoft.Extensions.Options;
using MovManagerr.Tmdb.Service;
using TMDbLib.Client;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.Search;

namespace MovManagerr.Tmdb
{
    public class TmdbClientService
    {
        private readonly TMDbClient _client;
        public readonly FavoriteService Favorites;
        
        public TmdbClientService(IOptions<Tmdb.Config.TmdbConfig> config)
        {
            _client = new TMDbClient(config.Value.ApiKey, config.Value.UseSsl, config.Value.Url);
            _client.SetSessionInformationAsync(config.Value.Session, SessionType.UserSession).Wait();
            _client.DefaultLanguage = config.Value.Language;

            Favorites = new FavoriteService(this);
        }

        /// <summary>
        /// Get movie by name and year
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<SearchMovie?> GetMovieByNameAndYearAsync(string name, int year)
        {
            var search = await _client.SearchMovieAsync(name, 0, false, year);
            return search.Results.FirstOrDefault();
        }

        /// <summary>
        /// Get movie by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<SearchMovie?> GetMovieByNameAsync(string name)
        {
            var search = await _client.SearchMovieAsync(name, 0, true);
            return search.Results.FirstOrDefault();
        }

        public async Task<IEnumerable<SearchMovie?>> GetRelatedMovies(string name)
        {
            var search = await _client.SearchMovieAsync(name, 0, true);
            return search.Results;
        }



        public TMDbClient GetClient()
        {
            return _client;
        }

    }
}