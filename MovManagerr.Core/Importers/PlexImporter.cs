using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.Extensions;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Services.Movies;
using MovManagerr.Tmdb;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Libraries;
using Plex.Library.ApiModels.Servers;
using Plex.ServerApi.PlexModels.Media;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Importers
{
    public class PlexImporter : ImporterBase
    {
        private readonly IPlexFactory _plexFactory;

        private readonly PlexConfiguration _plexConfiguration;

        private readonly IMovieService _movieService;


        public PlexImporter(
            IPlexFactory plexFactory,
            DbContext dbContext,
            IMovieService movieService) : base(dbContext)
        {
            _plexFactory = plexFactory;
            _plexConfiguration = Preferences.Instance.Settings.PlexConfiguration;
            _movieService = movieService;
        }

        public override async Task Import(CancellationToken cancellationToken)
        {
            SimpleLogger.AddLog("Importation des medias Plex en cours...", LogType.Info);

            // or use and Plex Auth token
            Plex.Library.ApiModels.Accounts.PlexAccount account = _plexFactory
                .GetPlexAccount(_plexConfiguration.ApiKey);

            // Get my server
            var servers = await account.Servers();
            var myServers = servers?.Where(c => c.Owned == 1);

            cancellationToken.ThrowIfCancellationRequested();

            if (IsValidServer(myServers))
            {
                List<DownloadedContent> contentDownloadeds = _dbContext.DownloadedContents.Query().Where(x => x.MovieId != 0).ToList();

                Server server = myServers!.FirstOrDefault()!;
                
                foreach (LibraryBase library in await server.Libraries())
                {
                    if (library is MovieLibrary movieLibrary)
                    {
                        await ProceededMovieLibrary(contentDownloadeds, movieLibrary, cancellationToken);
                    }
                }
            }
            
            SimpleLogger.AddLog(new NotificationLog("Tâche terminé", "Les films de plex ont été importé dans la base de donnée."), LogType.Info);
        }

        private async Task ProceededMovieLibrary(List<DownloadedContent> contentDownloadeds, MovieLibrary movieLibrary, CancellationToken cancellationToken)
        {
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
                    await ProceedMovieMetaData(tmdbService, metadata, contentDownloadeds);
                    processedCount++;
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

        private async Task ProceedMovieMetaData(TmdbClientService tmdbClient, Metadata metadata, List<DownloadedContent> contentDownloadeds)
        {
            foreach (var fullpath in GetContentPaths(metadata))
            {
                if (!contentDownloadeds.Any(x => x.FullPath == fullpath))
                {
                    (string title, int year) = GetTitleAndYearFromMetadata(metadata);

                    if (await tmdbClient.GetMovieByNameAndYearAsync(title, year) is SearchMovie searchMovie)
                    {
                        Data.Movie movie = _movieService.GetMovieFromSearchMovie(searchMovie);
                        _dbContext.DownloadedContents.CreateAndScan(movie, fullpath);
                        SimpleLogger.AddLog($"Le film {title} a bien été importé!");
                    }
                    else
                    {
                        SimpleLogger.AddLog($"Impossible de trouver le film {title} ({year}) dans la base de donnée de TMDb", LogType.Warning);
                    }
                }
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
