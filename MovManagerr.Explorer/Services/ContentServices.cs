using MovManagerr.Explorer.Config;
using MovManagerr.Models;
using Plex.ServerApi.Clients;
using RadarrSharp;
using System.Text.RegularExpressions;

namespace MovManagerr.Explorer.Services
{
    public class ContentServices
    {
        public readonly ExplorerPathConfig _explorerConfig;
        public readonly RadarrClient _radarrClient;

        /// <summary>
        /// Gets the base path.
        /// </summary>
        /// <value>
        /// The base path.
        /// </value>
        public string BasePath => _explorerConfig.MovieBasePath;


        public ContentServices(ExplorerPathConfig explorerConfig, RadarrInstanceConfig radarrInstanceConfig)
        {
            _explorerConfig = explorerConfig;
            _radarrClient = new RadarrClient(radarrInstanceConfig.Server, radarrInstanceConfig.Port, radarrInstanceConfig.ApiKey, "", false);
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
        public IEnumerable<MovieDirectorySpec> GetAllMoviesFromFiles()
        {
            //get each folder in base folder
            var movieFolder = GetAllMoviesFromFolder();
            List<MovieDirectorySpec> movieFiles = new List<MovieDirectorySpec>();
            int index = 0;
            
            foreach (var item in movieFolder)
            {
                index++;
                
                //get bigest file in folder
                var files = Directory.GetFiles(item.Path, "*.*", SearchOption.TopDirectoryOnly);
                
                var movie = files.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                
                if (movie != null)
                {                    
                    MovieFile movieFile = new MovieFile();
                    movieFile.Name = Path.GetFileName(movie);
                    movieFile.AddedDate = File.GetCreationTime(movie);
                    movieFile.Gb = new FileInfo(movie).Length / 1024f / 1024f / 1024f;

                    movieFiles.Add(new MovieDirectorySpec()
                    {
                        Id = index,
                        DirectoryInfo = item,
                        File = movieFile,
                        NbFiles = files.Count()
                    });
                }
            }

            return movieFiles;
        }

        /// <summary>
        /// Deletes the bad movie.
        /// </summary>
        /// <returns>The number of deleted movies</returns>
        public int DeleteBadMovie()
        {
            var movies = GetAllMoviesFromFiles();
            var movieToDelete = new List<MovieDirectorySpec>();
            
            foreach (var movie in movies)
            {
                if (movie.File.IsWebRip())
                {
                    movieToDelete.Add(movie);
                }
            }

            movieToDelete = movieToDelete.OrderByDescending(x => x.File.AddedDate).ToList();
            foreach (var movie in movieToDelete)
            {
                Directory.Delete(movie.DirectoryInfo.Path, true);
            }

            return movieToDelete.Count;
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
    }
}