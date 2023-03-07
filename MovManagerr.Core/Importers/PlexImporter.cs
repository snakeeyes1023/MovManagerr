using Hangfire;
using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Services.Movies;
using MovManagerr.Tmdb;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.ApiModels.Libraries;
using Plex.ServerApi.PlexModels.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Importers
{
    public class PlexImporter : ImporterBase
    {
        private readonly IPlexFactory _plexFactory;

        private readonly PlexConfiguration _plexConfiguration;

        private readonly IMovieService _movieService;

        public PlexImporter(IPlexFactory plexFactory, IContentDbContext contentDbContext, IMovieService movieService) : base(contentDbContext)
        {
            _plexFactory = plexFactory;
            _plexConfiguration = Preferences.Instance.Settings.PlexConfiguration;
            _movieService = movieService;
        }

        public override async Task Import(CancellationToken cancellationToken)
        {
            SimpleLogger.AddLog("Importation des medias Plex en cours...", LogType.Info);
            TmdbClientService tmdbClient = Preferences.GetTmdbInstance();

            // or use and Plex Auth token
            PlexAccount account = _plexFactory
                .GetPlexAccount(_plexConfiguration.ApiKey);

            // Get my server
            var servers = await account.Servers();
            var myServer = servers.Where(c => c.Owned == 1).FirstOrDefault();

            cancellationToken.ThrowIfCancellationRequested();

            if (myServer != null)
            {
                List<LibraryBase> libraries = await myServer.Libraries();

                foreach (LibraryBase library in libraries)
                {
                    if (library is MovieLibrary movieLibrary)
                    {
                        var mediasContainer = await movieLibrary.AllMovies("addedAt:desc", count: 40000);

                        foreach (Metadata metadata in mediasContainer.Media)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            string originalTitle = metadata.OriginalTitle ?? metadata.Title;
                            int year = metadata.Year;

                            SearchMovie? searchMovie = await tmdbClient.GetMovieByNameAndYearAsync(originalTitle, year);

                            if (searchMovie != null)
                            {
                                Movie movie = _movieService.GetMovieFromSearchMovie(searchMovie);

                                string fullPath = string.Empty;

                                foreach (var media in metadata.Media)
                                {
                                    foreach (var part in media.Part)
                                    {
                                        fullPath = _plexConfiguration.GetEquivalentPath(part.File);

                                        if (!movie.DownloadedContents.Any(x => x.FullPath == fullPath))
                                        {
                                            _movieService.CreateDownloadedContent(movie, fullPath, fullPath);
                                            SimpleLogger.AddLog($"Le film {originalTitle} a bien été importé!");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SimpleLogger.AddLog($"Impossible de trouver le film {originalTitle} ({year}) dans la base de donnée de TMDb", LogType.Warning);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Aucun server trouvé sur le compte Plex. Vous devez en être le propriétaire");
            }

            SimpleLogger.AddLog(new NotificationLog("Tâche terminé", "Les films de plex ont été importé dans la base de donnée."), LogType.Info);
        }
    }
}
