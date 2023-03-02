using Hangfire;
using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Services.Bases.ContentService;
using MovManagerr.Core.Tasks.Backgrounds;
using MovManagerr.Core.Tasks.Backgrounds.ContentTasks;
using MovManagerr.Core.Tasks.Backgrounds.MovieTasks;
using MovManagerr.Tmdb;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MovManagerr.Core.Services.Movies
{
    public class MovieService : BaseContentService<Movie>, IMovieService
    {
        private readonly IServiceProvider _serviceProvider;

        public MovieService(IServiceProvider serviceProvider, IContentDbContext contentDbContext) : base(contentDbContext)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Movie> GetRecent(int limit)
        {
            return _currentCollection.UseQuery(x =>
            {
                x.Limit(limit);
                BaseOrderQuery(x);
            }).ToList();
        }

        public EventedBackgroundService GetSearchAllMovieOnTmdbService()
        {
            var service = (EventedBackgroundService?)_serviceProvider.GetService(typeof(SearchAllMoviesOnTmdb));

            if (service == null)
            {
                throw new InvalidCastException("Impossible de trouver la tâche");
            }

            return service;
        }

        public EventedBackgroundService GetSyncM3UFilesInDbService()
        {
            var service = (EventedBackgroundService?)_serviceProvider.GetService(typeof(SyncM3UFiles));

            if (service == null)
            {
                throw new InvalidCastException("Impossible de trouver la tâche");
            }

            return service;
        }

        public Movie? GetMovieById(ObjectId _id)
        {
            Movie? movie = _currentCollection.UseQuery(x =>
            {
                x.Where(x => x._id == _id).FirstOrDefault();

            }).FirstOrDefault();

            if (movie != null && !movie.IsSearchedOnTmdb())
            {
                movie.SearchMovieOnTmdb();

                movie.SetDirty(true);

                _currentCollection.SaveChanges();
            }

            return movie;
        }

        public void Schedule_ScanFolder(string path)
        {
            BackgroundJob.Enqueue(() => ScanFolder(path));
        }

        [Queue("sync-task")]
        public void ScanFolder(string path)
        {
            var alreadyMapMovies = _currentCollection.ToList();

            if (Preferences.GetTmdbInstance() is TmdbClientService client)
            {
                // Récupérer la liste de tous les dossiers dans le chemin spécifié
                string[] directories = Directory.GetDirectories(path);
                Parallel.ForEach(directories, dirPath =>
                {
                    // obtenir le premier fichier dans le dossier actuel
                    var files = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);

                    if (files.Any() && TryExtractMovieNameAndYear(dirPath, out string title, out int year))
                    {
                        var TmdbMovie = client.GetMovieByNameAndYear(title, year);

                        if (TmdbMovie != null)
                        {
                            var movie = alreadyMapMovies.FirstOrDefault(x => x.TmdbId == TmdbMovie.Id);

                            if (movie != null)
                            {
                                if (!movie.DownloadedContents.Any(x => x.FullPath == files[0]))
                                {
                                    movie.DownloadedContents.Add(new Data.Abstracts.DownloadedContent(files[0]));
                                    movie.SetDirty();
                                }
                            }
                            else
                            {
                                movie = Movie.CreateFromSearchMovie(TmdbMovie);
                                movie.DownloadedContents.Add(new Data.Abstracts.DownloadedContent(files[0]));
                                _currentCollection.Add(movie);
                            }
                        }
                        else
                        {
                            SimpleLogger.AddLog($"Impossible de trouver le film {title} sur Tmdb", LogType.Error);
                        }
                    }

                });
            }

            _currentCollection.SaveChanges();
        }

        public void Schedule_ReorganiseFolder()
        {
            BackgroundJob.Enqueue(() => ReorganiseFolder());
        }

        [Queue("sync-task")]
        public void ReorganiseFolder()
        {
            var downloadedMovies = _currentCollection.ToList().Where(x => x.DownloadedContents.Any()).ToList();

            foreach (var movie in downloadedMovies)
            {
                try
                {
                    if (!movie.IsInValidFolder())
                    {
                        foreach (var download in movie.DownloadedContents)
                        {
                            Thread.Sleep(200);

                            var newPath = movie.GetFullPath(Path.GetFileName(download.FullPath), createDirectory: true);

                            if (download.FullPath != newPath)
                            {
                                var parentFolder = Path.GetDirectoryName(download.FullPath)!;

                                if (Directory.EnumerateFileSystemEntries(parentFolder).Count() == 1)
                                {
                                    //déplacement du fichier
                                    SimpleLogger.AddLog("Déplacement de " + download.FullPath + " vers " + newPath, LogType.Info);
                                    File.Move(download.FullPath, newPath);

                                    Thread.Sleep(200);

                                    //suppression du dossier parent
                                    SimpleLogger.AddLog("Suppression du dossier " + parentFolder, LogType.Info);
                                    Directory.Delete(parentFolder);

                                    download.FullPath = newPath;
                                    movie.SetDirty();
                                }
                                else
                                {
                                    SimpleLogger.AddLog("Impossible de traiter le dossier " + parentFolder + " car il contient d'autres fichiers", LogType.Error);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.AddLog("Erreur lors du traitement du film " + movie.Name + " : " + ex.Message, LogType.Error);
                }
            }
            _currentCollection.SaveChanges();
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
    }
}
