using FluentFTP;
using Microsoft.Extensions.Logging;
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
        public readonly FtpClient _ftpClient;
        private readonly ILogger<ContentServices> _logger;
        public string BasePath => _explorerConfig.MovieBasePath;


        public ContentServices(
            ExplorerPathConfig explorerConfig,
            FtpConfig ftpConfig,
            RadarrInstanceConfig radarrInstanceConfig,
            ILogger<ContentServices> logger,
            TmdbClientService tmdbService)
        {
            _explorerConfig = explorerConfig;
            _radarrClient = new RadarrClient(radarrInstanceConfig.Server, radarrInstanceConfig.Port, radarrInstanceConfig.ApiKey, "", false);
            _tmdbClient = tmdbService;
            _logger = logger;
            _ftpClient = new FtpClient(ftpConfig.Host, ftpConfig.Port, ftpConfig.User, ftpConfig.Password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<MovieDirectory> GetAllMoviesFromFolderAsync()
        {
            await _ftpClient.AutoConnectAsync();

            //get each folder in base folder
            foreach (FtpListItem item in await _ftpClient.GetListingAsync(BasePath))
            {
                // if this is a file
                if (item.Type == FtpFileSystemObjectType.Directory && item.Name.LastOrDefault() == ')')
                {
                    var extractedMovie = ExtractMovieFromFileName(item.Name, item.FullName);

                    if (extractedMovie != null)
                    {
                        yield return extractedMovie;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<MovieDirectorySpec> GetAllMoviesFromFilesAsync()
        {

            await _ftpClient.AutoConnectAsync();
            var likedMovies = await _tmdbClient.Favorites.GetFavoriteMoviesAsync();

            //get each folder in base folder
            foreach (FtpListItem item in await _ftpClient.GetListingAsync(BasePath))
            {
                // if this is a file
                if (item.Type == FtpFileSystemObjectType.Directory && item.Name.LastOrDefault() == ')')
                {
                    var extractedMovie = ExtractMovieFromFileName(item.Name, item.FullName);

                    if (extractedMovie != null)
                    {
                        var tmdbInfo = likedMovies.FirstOrDefault(m => m.OriginalTitle == extractedMovie.Title
                            && m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == extractedMovie.Year);

                        if (tmdbInfo != null)
                        {
                            long size = await _ftpClient.GetFileSizeAsync(item.FullName);

                            yield return new MovieDirectorySpec(extractedMovie, tmdbInfo, size);
                        }                
                    }
                }
            }
        }


        /// <summary>
        /// Extracts the name of from file.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public MovieDirectory? ExtractMovieFromFileName(string input, string path)
        {
            try
            {
                string year = input.Substring(input.Length - 7, 6).Substring(2);
                string movieName = input.Remove(input.Length - 7);

                return new MovieDirectory(movieName, path, year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while extracting movie name from file name");

                return default;
            }
        }

        public async Task DeleteElementAsync(string path)
        {
            await _ftpClient.AutoConnectAsync();
            await _ftpClient.DeleteFileAsync(path);
        }
    }
}