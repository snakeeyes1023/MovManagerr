using Hangfire;
using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.ApiModels.Libraries;
using Plex.Library.Factories;
using Plex.ServerApi.PlexModels.Media;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Threading;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.Search;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MovManagerr.Core.Tasks
{
    public class ContentAddService
    {
        public static event EventHandler<ContentTransfert>? ContentTransfered;

        private readonly IContentDbContext _contentDbContext;
        private readonly IPlexFactory _plexFactory;
        private readonly MovManagerr.Core.Infrastructures.Configurations.Preferences _preference;

        public ContentAddService(IContentDbContext contentDbContext, IPlexFactory plexFactory)
        {
            _contentDbContext = contentDbContext;
            _plexFactory = plexFactory;

            _preference = MovManagerr.Core.Infrastructures.Configurations.Preferences.Instance;
        }

        public void ImportMovie(string fullPath, SearchMovie info)
        {
            Movie? movie = GetMovieFromSearchMovie(info);

            ContentTransfert transfertInfo = new ContentTransfert()
            {
                DeleteOrigin = false,
                Destination = movie.GetFullPath(Path.GetFileName(fullPath)),
                Origin = fullPath,
                _id = movie._id
            };

            if (movie.DownloadedContents.FirstOrDefault(x => x.FullPath == transfertInfo.Destination) is DownloadedContent alreadyExist)
            {
                SimpleLogger.AddLog("Le fichier existe déjà dans la base de donnée", LogType.Warning);

                if (!File.Exists(transfertInfo.Destination))
                {
                    TransfertAndReencodeIfRequired(transfertInfo, alreadyExist);

                    //todo rescanner le fichier
                }
            }
            else
            {
                DownloadedContent downloadedContent = CreateDownloadedContent(movie, transfertInfo);

                if (File.Exists(transfertInfo.Destination))
                {
                    SimpleLogger.AddLog("Impossible de transférer puisqu'un fichier du même nom est déjà dans le répertoire (renommer ou supprimer l'élément déjà dans le répertoire)", LogType.Warning);
                }
                else
                {
                    TransfertAndReencodeIfRequired(transfertInfo, downloadedContent);
                }
            }
        }


        private void TransfertAndReencodeIfRequired(ContentTransfert transfertInfo, DownloadedContent downloadedContent)
        {
            if (_preference.Settings.TranscodeConfiguration.IsTranscodeRequired(downloadedContent))
            {
                BackgroundJob.Enqueue(() => ReencodeAndTransfert(transfertInfo, CancellationToken.None));
            }
            else
            {
                BackgroundJob.Enqueue(() => Transfert(transfertInfo));
            }
        }

        public void Schedule_ImportFromPlex()
        {
            BackgroundJob.Enqueue(() => ImportFromPlex(CancellationToken.None));
        }

        [Queue("sync-task")]
        public async Task ImportFromPlex(CancellationToken cancellationToken)
        {
            SimpleLogger.AddLog("Importation des medias Plex en cours...", LogType.Info);
            TmdbClientService tmdbClient = Preferences.GetTmdbInstance();

            // or use and Plex Auth token
            PlexAccount account = _plexFactory
                .GetPlexAccount(_preference.Settings.PlexConfiguration.ApiKey);

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
                                Movie movie = GetMovieFromSearchMovie(searchMovie);

                                string fullPath = string.Empty;

                                foreach (var media in metadata.Media)
                                {
                                    foreach (var part in media.Part)
                                    {
                                        fullPath = _preference.Settings.PlexConfiguration.GetEquivalentPath(part.File);

                                        if (!movie.DownloadedContents.Any(x => x.FullPath == fullPath))
                                        {
                                            CreateDownloadedContent(movie, fullPath);
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

            SimpleLogger.AddLog("Importation terminée", LogType.Info);
        }

        private DownloadedContent CreateDownloadedContent(Movie movie, string path)
        {
            return CreateDownloadedContent(movie, path, path);
        }

        private DownloadedContent CreateDownloadedContent(Movie movie, ContentTransfert transfertInfo)
        {
            return CreateDownloadedContent(movie, transfertInfo.Origin, transfertInfo.Destination);
        }

        private DownloadedContent CreateDownloadedContent(Movie movie, string loadInfoFrom, string path)
        {
            var downloadedContent = new DownloadedContent(path);

            try
            {
                downloadedContent.LoadMediaInfo(loadInfoFrom);
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog("Impossible de charger les informations de la vidéo" + loadInfoFrom + " : " + ex.Message, LogType.Warning);
            }

            movie.DownloadedContents.Add(downloadedContent);

            // save the movie in the database
            _contentDbContext.Movies.TrackEntity(movie);
            movie.SetDirty();
            _contentDbContext.Movies.SaveChanges();

            return downloadedContent;
        }


        
        private Movie GetMovieFromSearchMovie(SearchMovie info)
        {
            var movie = _contentDbContext.Movies.UseQuery(x =>
            {
                x.Where(movie => movie.TmdbId == info.Id);
            }).ToList().FirstOrDefault();

            // Add the movie if not exists
            if (movie == null)
            {
                movie = Movie.CreateFromSearchMovie(info);
                _contentDbContext.Movies.Add(movie);
                _contentDbContext.Movies.SaveChanges();
            }

            return movie;
        }

        private bool MustBeReencode(DownloadedContent content)
        {
            return true;
        }
        
        [Queue("reencoding")]
        [DisableConcurrentExecution(800 * 60)]
        public async Task ReencodeAndTransfert(ContentTransfert transfertInfo, CancellationToken cancellationToken)
        {
            string transcodeDestination = Path.Combine(Path.GetDirectoryName(transfertInfo.Origin), Path.GetFileNameWithoutExtension(transfertInfo.Origin) + " [Transcode]" + Path.GetExtension(transfertInfo.Origin));

            SimpleLogger.AddLog($"Ré-encodage du film en cours vers {transcodeDestination} ...");

            string ffmpegString = _preference.Settings.TranscodeConfiguration.GetTranscodeFFmpegString(transfertInfo.Origin, transcodeDestination);

            IConversionResult conversionResult = await FFmpeg.Conversions.New().Start(ffmpegString, cancellationToken);

            SimpleLogger.AddLog($"Ré-encodage terminé après {conversionResult.Duration.ToString("h'h 'm'm 's's'")}", LogType.Info);

            transfertInfo.Origin = transcodeDestination;

            SimpleLogger.AddLog($"Transfert en file d'attente");
            //BackgroundJob.Enqueue(() => Transfert(transfertInfo));
        }

        [Queue("file-transfert")]
        public void Transfert(ContentTransfert transfertInfo)
        {
            SimpleLogger.AddLog($"Déplacement du fichier {transfertInfo.Origin} vers {transfertInfo.Destination} en cours...");

            File.Move(transfertInfo.Origin, transfertInfo.Destination);

            SimpleLogger.AddLog($"Déplacement du fichier {transfertInfo.Origin} vers {transfertInfo.Destination} terminée", LogType.Info);

            ContentTransfered?.Invoke(this, transfertInfo);
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

    public class ContentTransfert
    {
        public ObjectId _id { get; set; }
        public bool DeleteOrigin { get; set; } = false;
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string ContentName { get; set; }
    }
}
