using Hangfire;
using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using System.IO;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Tasks
{
    public class ContentAddService
    {
        public static event EventHandler<ContentTransfert>? ContentTransfered;

        private readonly IContentDbContext _contentDbContext;

        public ContentAddService(IContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
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

            if (File.Exists(transfertInfo.Destination))
            {
                SimpleLogger.AddLog("Le fichier existe déjà", LogType.Error);
                return;
            }

            DownloadedContent downloadedContent = CreateDownloadedContent(movie, transfertInfo);

            if (MustBeReencode(downloadedContent))
            {
                BackgroundJob.Enqueue(() => ReencodeAndTransfert(transfertInfo));
            }
            else
            {
                BackgroundJob.Enqueue(() => Transfert(transfertInfo));
            }
        }


        private DownloadedContent CreateDownloadedContent(Movie movie, ContentTransfert transfertInfo)
        {
            var downloadedContent = new DownloadedContent(transfertInfo.Destination);

            try
            {
                downloadedContent.LoadMediaInfo(transfertInfo.Origin);
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog("Impossible de charger les informations de la vidéo" + transfertInfo.Origin + " : " + ex.Message, LogType.Warning);
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
            return false;
        }

        public void ReencodeAndTransfert(ContentTransfert transfertInfo)
        {
            // re-encode
            Transfert(transfertInfo);
        }

        public void Transfert(ContentTransfert transfertInfo)
        {
            File.Move(transfertInfo.Origin, transfertInfo.Destination);

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
