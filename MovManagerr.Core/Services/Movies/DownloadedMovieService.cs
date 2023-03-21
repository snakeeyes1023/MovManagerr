using Hangfire;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Core.Services.Movies
{
    public class DownloadedMovieService : IDownloadedMovieService
    {
        private readonly IMovieRepository _movieRepository;
        
        public DownloadedMovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public void Schedule_DeleteUnfoundedDownload()
        {
            BackgroundJob.Enqueue(() => DeleteUnfoundedDownload());
        }

        public void DeleteUnfoundedDownload()
        {
            foreach (var movie in _movieRepository.All())
            {
                if (movie != null)
                {
                    List<DownloadedContent> toDelete = new List<DownloadedContent>();
                    foreach (var download in movie.DownloadedContents)
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
                            movie.DownloadedContents.Remove(download);
                        }
                    }
                }
            }

            // Save changes
        }
    }
}
