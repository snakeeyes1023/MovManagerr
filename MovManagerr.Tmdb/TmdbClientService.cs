using Microsoft.Extensions.Options;
using MovManagerr.Tmdb.Config;
using MovManagerr.Tmdb.Service;
using TMDbLib.Client;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.General;
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

            // get perfect match
            var perfectMatch = search.Results.FirstOrDefault(x => x.Title.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (perfectMatch != null)
            {
                return perfectMatch;
            }

            return search.Results.FirstOrDefault();
        }

        public SearchMovie? GetMovieByName(string name)
        {
            Task<SearchMovie?> task = Task.Run<SearchMovie?>(async () => await GetMovieByNameAsync(name));
            return task.Result;
        }

        public SearchMovie? GetMovieByNameAndYear(string name, int year)
        {
            Task<SearchMovie?> task = Task.Run<SearchMovie?>(async () => await GetMovieByNameAndYearAsync(name, year));
            return task.Result;
        }

        public IEnumerable<SearchMovie?>? GetRelatedMovies(string name)
        {
            Task<SearchContainer<SearchMovie>?> task = Task.Run<SearchContainer<SearchMovie>?>(async () => await _client.SearchMovieAsync(name, 0, true));
           return task.Result?.Results;
        }

        public TMDbClient GetClient()
        {
            return _client;
        }

    }
}