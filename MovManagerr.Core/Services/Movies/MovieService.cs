using Hangfire;
using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Dbs.Repositories;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using System.Text.RegularExpressions;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Services.Movies
{
    public class MovieService : IMovieService
    {
        private readonly DbContext _dbContext;

        public MovieService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Queue("sync-task")]
        public void ScanFolder(string path)
        {
            //var alreadyMapMovies = _dbContext.All();

            //if (Preferences.GetTmdbInstance() is TmdbClientService client)
            //{
            //    // Récupérer la liste de tous les dossiers dans le chemin spécifié
            //    string[] directories = Directory.GetDirectories(path);
            //    Parallel.ForEach(directories, dirPath =>
            //    {
            //        // obtenir le premier fichier dans le dossier actuel
            //        var files = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);

            //        if (files.Any() && TryExtractMovieNameAndYear(dirPath, out string title, out int year))
            //        {
            //            var TmdbMovie = client.GetMovieByNameAndYear(title, year);

            //            if (TmdbMovie != null)
            //            {
            //                var movie = alreadyMapMovies.FirstOrDefault(x => x.TmdbId == TmdbMovie.Id);

            //                if (movie != null)
            //                {
            //                    if (!movie.Medias.Any(x => x.FullPath == files[0]))
            //                    {
            //                        movie.Medias.Add(new DownloadedContent(files[0]));
            //                    }
            //                }
            //                else
            //                {
            //                    movie = Movie.CreateFromSearchMovie(TmdbMovie);
            //                    movie.Medias.Add(new DownloadedContent(files[0]));
            //                    _movieRepository.Create(movie);
            //                }
            //            }
            //            else
            //            {
            //                SimpleLogger.AddLog($"Impossible de trouver le film {title} sur Tmdb", LogType.Error);
            //            }
            //        }

            //    });
            //}
        }

        public void Schedule_ReorganiseFolder()
        {
            BackgroundJob.Enqueue(() => ReorganiseFolder());
        }

        [Queue("sync-task")]
        public void ReorganiseFolder()
        {
            //var downloadedMovies = _movieRepository
            //    .Query()
            //    .Where(x => x.Medias.Any())
            //    .ToList();

            //foreach (var movie in downloadedMovies)
            //{
            //    try
            //    {
            //        if (!movie.IsInValidFolder())
            //        {
            //            foreach (var download in movie.Medias)
            //            {
            //                Thread.Sleep(200);

            //                var newPath = movie.GetFullPath(Path.GetFileName(download.FullPath), createDirectory: true);

            //                if (download.FullPath != newPath)
            //                {
            //                    var parentFolder = Path.GetDirectoryName(download.FullPath)!;

            //                    if (Directory.EnumerateFileSystemEntries(parentFolder).Count() == 1)
            //                    {
            //                        //déplacement du fichier
            //                        SimpleLogger.AddLog("Déplacement de " + download.FullPath + " vers " + newPath, LogType.Info);
            //                        File.Move(download.FullPath, newPath);

            //                        Thread.Sleep(200);

            //                        //suppression du dossier parent
            //                        SimpleLogger.AddLog("Suppression du dossier " + parentFolder, LogType.Info);
            //                        Directory.Delete(parentFolder);

            //                        download.FullPath = newPath;
            //                    }
            //                    else
            //                    {
            //                        SimpleLogger.AddLog("Impossible de traiter le dossier " + parentFolder + " car il contient d'autres fichiers", LogType.Error);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        SimpleLogger.AddLog("Erreur lors du traitement du film " + movie.Name + " : " + ex.Message, LogType.Error);
            //    }
            //}
        }

        private static bool TryExtractMovieNameAndYear(string dirPath, out string title, out int year)
        {
            title = string.Empty;
            year = 0;

            try
            {
                // Extraire le nom du dossier
                string dirName = Path.GetFileName(dirPath);

                // Vérifier si le nom de dossier est dans le format attendu
                var regex = new Regex(@"^(.*)\s\((\d{4})\)$");
                Match match = regex.Match(dirName);

                if (match.Success)
                {
                    // Extraire le titre et l'année
                    title = match.Groups[1].Value;
                    year = int.Parse(match.Groups[2].Value);

                    // Faire quelque chose avec le titre et l'année (par exemple, les afficher dans la console)
                    SimpleLogger.AddLog($"Titre : {title}, Année : {year}", LogType.Info);

                    return true;
                }
                else
                {
                    SimpleLogger.AddLog($"Nom de dossier non conforme : {dirName}", LogType.Warning);
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs éventuelles
                SimpleLogger.AddLog($"Erreur lors du traitement du dossier : {dirPath}", LogType.Error);
                SimpleLogger.AddLog(ex.Message, LogType.Error);
            }

            return false;
        }


        public void Schedule_DeleteUnfoundedDownload()
        {
            BackgroundJob.Enqueue(() => DeleteUnfoundedDownload());
        }

        public void DeleteUnfoundedDownload()
        {
            var movies = _dbContext.Movies.GetDownloadedMovies();

            foreach (var movie in movies)
            {
                if (movie != null)
                {
                    List<DownloadedContent> toDelete = new List<DownloadedContent>();
                    foreach (var download in movie.Medias)
                    {
                        if (download != null && !File.Exists(download.FullPath))
                        {
                            SimpleLogger.AddLog($"Le fichier {download.FullPath} est introuvable! Suppression imminente.");
                            toDelete.Add(download);
                        }
                    }

                    if (toDelete.Any())
                    {
                        foreach (var download in toDelete)
                        {
                            movie.Medias.Remove(download);
                        }
                    }
                }
            }

            // Save changes
        }


        public Movie GetMovieFromSearchMovie(SearchMovie info)
        {
            Movie movie = _dbContext.Movies.FindByTmdbId(info.Id);

            // Add the movie if not exists
            if (movie == null)
            {
                movie = _dbContext.Movies.Create(Movie.CreateFromSearchMovie(info));
            }

            return movie;
        }

        public IEnumerable<SearchMovie?> GetMatchForFileName(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename) && Preferences.GetTmdbInstance() is TmdbClientService client)
            {
                return client.GetRelatedMovies(filename) ?? new List<SearchMovie>();
            }

            return new List<SearchMovie>();
        }
    }
}
