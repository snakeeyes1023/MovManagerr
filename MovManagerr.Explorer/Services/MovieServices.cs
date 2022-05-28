using Microsoft.Extensions.Logging;
using MovManagerr.Models;
using MovManagerr.Tmdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Explorer.Services
{
    public class MovieServices
    {
        private readonly ContentServices _contentServices;
        private readonly TmdbClientService _tmdbClient;
        private readonly ILogger<MovieServices> _logger;

        public MovieServices(ContentServices contentService, TmdbClientService tmdbClient, ILogger<MovieServices> logger)
        {
            _contentServices = contentService;
            _tmdbClient = tmdbClient;
            _logger = logger;
        }

        /// <summary>
        /// Sync The movie to Radarr.
        /// </summary>
        /// <returns></returns>
        public async Task SyncMovieListByFolderAsync()
        {
            var downloadedMovie = _contentServices.GetAllMoviesFromFolderAsync();

            await foreach (var movie in downloadedMovie)
            {
                try
                {
                    if (!await _tmdbClient.Favorites.LikeMovieByNameAndYearAsync(movie.Title, movie.Year))
                    {
                        throw new System.Exception("Movie not found");
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while syncing movie");
                }
            }
        }



        /// <summary>
        /// Delete Movie with the key ImbdID
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task FullDeleteMovie(int key)
        {
            var movies = _contentServices.GetAllMoviesFromFilesAsync();

            await foreach (var movieData in movies)
            {
                if (movieData.Id == key)
                {
                    await _tmdbClient.Favorites.DislikeMovieAsync(movieData.Id);
                    await _contentServices.DeleteElementAsync(movieData.FullPath);
                    return;
                }
            }
        }


        /// <summary>
        /// Deletes the bad movie.
        /// </summary>
        /// <returns>The number of deleted movies</returns>
        public async Task DeleteBadMovie()
        {
            var movies = _contentServices.GetAllMoviesFromFilesAsync();

            await foreach (var movie in movies)
            {
                 await DeleteMovie(movie);
            }
        }


        /// <summary>
        /// Delete movie on the folder 
        /// </summary>
        /// <param name="movieToDelete"></param>
        /// <param name="movie"></param>
        /// <returns></returns>
        private async Task<int> DeleteMovie(MovieDirectorySpec movie)
        {
            if (movie.IsWebRip())
            {
                await _contentServices.DeleteElementAsync(movie.FullPath);
            }

            if (movie.Movie != null && movie.Movie.OriginalLanguage == "ja")
            {
                await FullDeleteMovie(movie.Id);
            }

            return movie.Id;
        }
    }
}
