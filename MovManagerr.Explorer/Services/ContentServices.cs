using MovManagerr.Explorer.Config;
using MovManagerr.Models;
using MovManagerr.Tmdb;
using Plex.ServerApi.Clients;
using RadarrSharp;
using System.Text.RegularExpressions;

namespace MovManagerr.Explorer.Services
{
    public class ContentServices
    {
        public readonly ExplorerPathConfig _explorerConfig;
        public readonly RadarrClient _radarrClient;
        public readonly TmdbClientService _tmdbClient;

        /// <summary>
        /// Gets the base path.
        /// </summary>
        /// <value>
        /// The base path.
        /// </value>
        public string BasePath => _explorerConfig.MovieBasePath;


        public ContentServices(ExplorerPathConfig explorerConfig,
            RadarrInstanceConfig radarrInstanceConfig,
            TmdbClientService tmdbService)
        {
            _explorerConfig = explorerConfig;
            _radarrClient = new RadarrClient(radarrInstanceConfig.Server, radarrInstanceConfig.Port, radarrInstanceConfig.ApiKey, "", false);
            _tmdbClient = tmdbService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MovieDirectory> GetAllMoviesFromFolder()
        {
            //get each folder in base folder
            List<MovieDirectory> movieDirectories = new List<MovieDirectory>();
            string[] dirs = Directory.GetDirectories(BasePath, "*)", SearchOption.TopDirectoryOnly);

            foreach (var dir in dirs)
            {
                //remove base path from name
                string folderName = dir.Split(BasePath).LastOrDefault() ?? "";

                var movie = ExtractFromFileName(folderName.Substring(1));
                movie.Path = dir;
                movieDirectories.Add(movie);
            }

            return movieDirectories;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MovieDirectorySpec>> GetAllMoviesFromFilesAsync()
        {
            //get each folder in base folder
            var movieFolder = GetAllMoviesFromFolder();
            var likedMovies = await _tmdbClient.Favorites.GetFavoriteMoviesAsync();

            List<MovieDirectorySpec> movieFiles = new List<MovieDirectorySpec>();
            int index = 0;

            foreach (var item in movieFolder)
            {
                index++;

                var tmdbInfo = likedMovies.Where(m => m.OriginalTitle == item.Title && m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == item.Year).FirstOrDefault();

                if (tmdbInfo != null)
                {
                    movieFiles.Add(new MovieDirectorySpec(item, tmdbInfo));
                }
            }

            return movieFiles;
        }


        public async Task DeleteMovie(int key)
        {
            var movies = await GetAllMoviesFromFilesAsync();
            var movie = movies.FirstOrDefault(m => m.Id == key);

            if (movie != null)
            {
                await _tmdbClient.Favorites.DislikeMovieAsync(movie.Id);
                Directory.Delete(movie.DirectoryInfo.Path, true);
            }
        }


        /// <summary>
        /// Deletes the bad movie.
        /// </summary>
        /// <returns>The number of deleted movies</returns>
        public async Task<int> DeleteBadMovie()
        {
            var movies = await GetAllMoviesFromFilesAsync();
            var movieToDelete = new List<MovieDirectorySpec>();

            foreach (var movie in movies)
            {
                await DeleteMovie(movieToDelete, movie);
            }

            foreach (var movie in movieToDelete)
            {
                Directory.Delete(movie.DirectoryInfo.Path, true);
            }

            return movieToDelete.Count;
        }

        private async Task DeleteMovie(List<MovieDirectorySpec> movieToDelete, MovieDirectorySpec movie)
        {
            if (movie.IsWebRip())
            {
                movieToDelete.Add(movie);
            }

            if (movie.Movie != null && movie.Movie.OriginalLanguage == "ja")
            {
                movieToDelete.Add(movie);
                await _tmdbClient.Favorites.DislikeMovieAsync(movie.Movie.Id);
            }
        }


        /// <summary>
        /// Extracts the name of from file.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public MovieDirectory ExtractFromFileName(string input)
        {
            try
            {
                string year = input.Substring(input.Length - 7, 6).Substring(2);
                string movieName = input.Remove(input.Length - 7);

                return new MovieDirectory()
                {
                    Title = movieName,
                    Year = int.Parse(year)
                };
            }
            catch (Exception)
            {
                return new MovieDirectory()
                {
                    Title = input,
                    Year = 0
                };
            }
        }


        /// <summary>
        /// Sync The movie to Radarr.
        /// </summary>
        /// <returns></returns>
        public async Task SyncMovieListByFolderAsync()
        {
            var downloadedMovie = GetAllMoviesFromFolder();

            foreach (var movie in downloadedMovie)
            {
                try
                {
                    if (!await _tmdbClient.Favorites.LikeMovieByNameAndYearAsync(movie.Title, movie.Year))
                    {
                        throw new System.Exception("Movie not found");
                    };
                }
                catch
                {
                    //do nothing
                }

            }

        }

    }
}