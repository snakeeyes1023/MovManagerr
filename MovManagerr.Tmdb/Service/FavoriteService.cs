using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace MovManagerr.Tmdb.Service
{
    public class FavoriteService
    {
        private readonly TMDbClient _client;
        private readonly TmdbClientService _baseService;
        public FavoriteService(TmdbClientService client)
        {
            _client = client.GetClient();
            _baseService = client;
        }

        /// <summary>
        /// Like recommended movie
        /// </summary>
        /// <param name="nb"></param>
        /// <returns></returns>
        public async Task LikeNewMovieAsync(int nb = 2)
        {
            var likedMovies = await GetFavoriteMoviesAsync();
            var candidateMovies = await GetRecommandedMovieAsync(nb, likedMovies);

            //like randomly candidateMovies
            await LikeRandomlyMovieByListAsync(nb, candidateMovies);
        }

        /// <summary>
        /// Like random movie
        /// </summary>
        /// <param name="nb">nb of movie to like</param>
        /// <param name="candidateMovies"></param>
        /// <returns></returns>
        private async Task LikeRandomlyMovieByListAsync(int nb, List<SearchMovie> candidateMovies)
        {
            var random = new Random();

            for (int i = 0; i < nb; i++)
            {
                var hasSucceeded = false;
                while (!hasSucceeded)
                {
                    var movie = candidateMovies[random.Next(candidateMovies.Count)];
                    var result = await _client.AccountChangeFavoriteStatusAsync(movie.MediaType, movie.Id, true);
                    if (result)
                    {
                        hasSucceeded = true;
                        candidateMovies.Remove(movie);
                    }
                }
            }
        }

        /// <summary>
        /// Get recommandation movies
        /// </summary>
        /// <param name="nb"></param>
        /// <param name="likedMovies"></param>
        /// <returns></returns>
        public async Task<List<SearchMovie>> GetRecommandedMovieAsync(int nb, List<SearchMovie> likedMovies)
        {
            int page = 1;
            List<SearchMovie> candidateMovies = new List<SearchMovie>();
            
            while (candidateMovies.Count() < nb * 2)
            {
                var movie = await _client.GetMovieNowPlayingListAsync(null, page, "CA");

                //remove already liked movies
                candidateMovies.AddRange(movie.Results.Where(m => !likedMovies.Any(l => l.Id == m.Id) && m.OriginalLanguage != "ja").ToList());
                page++;
            }

            return candidateMovies;
        }

        /// <summary>
        /// Like a movie with is name and year
        /// </summary>
        /// <param name="name">Name of the movie</param>
        /// <param name="year">Release year</param>
        /// <returns></returns>
        public async Task<bool> LikeMovieByNameAndYearAsync(string name, int year)
        {
            SearchMovie? movie = await _baseService.GetMovieByNameAndYearAsync(name, year);

            if (movie == null)
            {
                return false;
            }

            return await _client.AccountChangeFavoriteStatusAsync(movie.MediaType, movie.Id, true);
        }

        public async Task<bool> DislikeMovieAsync(int id)
        {
            return await _client.AccountChangeFavoriteStatusAsync(TMDbLib.Objects.General.MediaType.Movie, id, false);
        }

        /// <summary>
        /// Get all favorite movies
        /// </summary>
        /// <returns>List of favorites movies</returns>
        public async Task<List<SearchMovie>> GetFavoriteMoviesAsync()
        {
            int totalPages = 1;
            List<SearchMovie> movies = new List<SearchMovie>();
            
            for (int i = 1; i <= totalPages; i++)
            {
                var page = await _client.AccountGetFavoriteMoviesAsync(i);
                movies.AddRange(page.Results);
                
                totalPages = page.TotalPages;
            }
            
            return movies;
        }
        
        /// <summary>
        /// Delete all favorites movies
        /// </summary>
        /// <returns>the number of flushed movie</returns>
        public async Task<int> FlushFavoriteMoviesAsync()
        {
            var movies = await GetFavoriteMoviesAsync();    
            var count = 0;

            foreach (var movie in movies)
            {
                try
                {
                    if (!await _client.AccountChangeFavoriteStatusAsync(movie.MediaType, movie.Id, false))
                    {
                        throw new Exception("Can't change favorite type");
                    }           
                }
                catch (Exception)
                {
                    count--;
                }
                count++;
            }
            return count;
        }
    }
}
