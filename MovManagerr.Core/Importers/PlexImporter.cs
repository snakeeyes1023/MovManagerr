using Hangfire;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.Extensions;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.DataAccess;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Infrastructures.TrackedTasks.Generals;
using MovManagerr.Core.Infrastructures.TrackedTasks;
using MovManagerr.Core.Services.Movies;
using MovManagerr.Tmdb;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Libraries;
using Plex.Library.ApiModels.Servers;
using Plex.ServerApi.PlexModels.Media;
using TMDbLib.Objects.Search;
using Hangfire.Server;

namespace MovManagerr.Core.Importers
{
    public class PlexImporter
    {
        private readonly IPlexFactory _plexFactory;

        private readonly PlexConfiguration _plexConfiguration;

        private readonly IMovieService _movieService;

        private readonly DbContext _dbContext;

        public PlexImporter(
            IPlexFactory plexFactory,
            DbContext dbContext,
            IMovieService movieService)
        {
            _plexFactory = plexFactory;
            _plexConfiguration = Preferences.Instance.Settings.PlexConfiguration;
            _movieService = movieService;
            _dbContext = dbContext;
        }


        public void EnqueueRun()
        {
            string jobId = BackgroundJob.Enqueue(() => Import(CancellationToken.None, null));
            
            CreateProgressJob(jobId);
        }


        private PlexImportJobProgression CreateProgressJob(string jobId)
        {
            return GlobalTrackedTask.AddTrackedJob(new PlexImportJobProgression(jobId));
        }

        [Queue("sync-task")]
        public async Task Import(CancellationToken cancellationToken, PerformContext? context)
        {
            SimpleLogger.AddLog("Importation des medias Plex en cours...", LogType.Info);

            // or use and Plex Auth token
            Plex.Library.ApiModels.Accounts.PlexAccount account = _plexFactory
                .GetPlexAccount(_plexConfiguration.ApiKey);

            // Get my server
            var servers = await account.Servers();
            var myServers = servers?.Where(c => c.Owned == 1);

            cancellationToken.ThrowIfCancellationRequested();



            PlexImportJobProgression progression;

            if (context != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is PlexImportJobProgression transfert)
            {
                progression = transfert;
            }
            else
            {
                progression = CreateProgressJob(context?.BackgroundJob.Id ?? string.Empty);
            }


            if (IsValidServer(myServers))
            {
                Server server = myServers!.FirstOrDefault()!;

                foreach (LibraryBase library in await server.Libraries())
                {
                    if (library is MovieLibrary movieLibrary)
                    {
                        await ProceededMovieLibrary(movieLibrary, progression, cancellationToken);
                    }
                }
            }

            SimpleLogger.AddLog(new NotificationLog("Tâche terminé", "Les films de plex ont été importé dans la base de donnée."), LogType.Info);

            progression.Progress = 100;
        }

        private async Task ProceededMovieLibrary(MovieLibrary movieLibrary, PlexImportJobProgression progression, CancellationToken cancellationToken)
        {
            progression.Message = $"Importation de la librairie {movieLibrary.Title}";
            progression.Progress = 0;

            var tmdbService = Preferences.GetTmdbInstance();
            int batchSize = 50;
            int startIndex = 0;
            int totalCount = 1; // nombre total de films dans la bibliothèque est directement récupéré dans la première requête
            int processedCount = 0;

            while (processedCount < totalCount)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mediasContainer = await movieLibrary.AllMovies("addedAt:desc", startIndex, batchSize);
                var mediaList = mediasContainer.Media.ToList();
                totalCount = mediasContainer.TotalSize;

                int batchCount = mediaList.Count;

                if (batchCount == 0) break; // fin de la liste de films

                for (int i = 0; i < batchCount; i++)
                {
                    var metadata = mediaList[i];
                    await ProceedMovieMetaData(tmdbService, metadata);
                    processedCount++;

                    progression.Progress = ((int)((double)processedCount / totalCount * 100) - 1);
                }

                startIndex += batchCount;
            }
        }

        private bool IsValidServer(IEnumerable<Server>? myServers)
        {
            if (myServers == null || myServers.Count() <= 0)
            {
                SimpleLogger.AddLog("Aucun serveur Plex trouvé. Vous devez en être le propriétaire pour lancer l'importation");
                return false;
            }
            if (myServers.Count() > 1)
            {
                SimpleLogger.AddLog("Plusieurs serveurs Plex ont été trouvé. Seul le premier sera utilisé pour l'importation");
                return true;
            }

            return true;
        }

        private async Task ProceedMovieMetaData(TmdbClientService tmdbClient, Metadata metadata)
        {

            foreach (var fullpath in GetContentPaths(metadata))
            {
                try
                {
                    var alreadyExists = _dbContext.Movies.FindByFullPath(fullpath);

                    if (alreadyExists == null)
                    {
                        if (await ExtractTmdbInfoFromMetaData(tmdbClient, metadata) is TMDbLib.Objects.Movies.Movie tmdbMovie)
                        {
                            Data.Movie movie = _movieService.GetMovieFromTDMBMovie(tmdbMovie);

                            movie.CreateAndScan(fullpath);

                            _dbContext.Movies.Update(movie);
                            
                            SimpleLogger.AddLog($"Le film {tmdbMovie.Title} a bien été importé!");
                        }
                        else
                        {
                            SimpleLogger.AddLog($"Impossible de trouver le film {fullpath} dans la base de donnée de TMDb", LogType.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.AddLog($"Impossible de trouver le film {fullpath} dans la base de donnée de TMDb", LogType.Warning);
                }

            }
        }

        private static async Task<TMDbLib.Objects.Movies.Movie?> ExtractTmdbInfoFromMetaData(TmdbClientService tmdbClient, Metadata metadata)
        {
            try
            {
                var scrapingId = metadata.ScrapingIds.FirstOrDefault(x => x.Id.StartsWith("tmdb"));
                if (scrapingId != null)
                {
                    int tmdbId = int.Parse(scrapingId.Id.Split("//")[1]);
                    return await tmdbClient.GetMovieByIdAsync(tmdbId);
                }
                else
                {
                    throw new InvalidOperationException("Impossible d'extraire l'id");
                }
            }
            catch (Exception)
            {
                (string title, int year) = GetTitleAndYearFromMetadata(metadata);
                return await tmdbClient.GetMovieByNameAndYearAsync(title, year);
            }
        }

        private IEnumerable<string> GetContentPaths(Metadata metadata)
        {
            foreach (var media in metadata.Media)
            {
                foreach (var part in media.Part)
                {
                    yield return _plexConfiguration.GetEquivalentPath(part.File);
                }
            }
        }

        private static (string, int) GetTitleAndYearFromMetadata(Metadata metadata)
        {
            string title;

            if (!string.IsNullOrWhiteSpace(metadata.OriginalTitle) && MovieTmdbExtensions.IsValidFolder(metadata.OriginalTitle))
            {
                title = metadata.OriginalTitle;
            }
            else
            {
                title = metadata.Title;
            }

            return (title, metadata.Year);
        }
    }
}
