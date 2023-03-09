using Hangfire;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using System.Net;
using System.Net.NetworkInformation;

namespace MovManagerr.Core.Downloaders.Contents
{
    public class ContentDownloaderClient
    {
        public DateTime StartedTime { get; set; }

        private readonly IContentDbContext ContentDbContext;

        public ContentDownloaderClient(IContentDbContext contentDbContext)
        {
            ContentDbContext = contentDbContext;
        }


        public void Download<T>(T content, DirectLinkDownload link) where T : Content
        {
            try
            {
                link.IsDownloading = true;

                var fullPath = content.GetFullPath(link.GetFilename());

                Start(new DownloadContentTask()
                {
                    Content = content,
                    Destination = fullPath,
                    IsFinish = false,
                    Origin = link
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>("Impossible de créer le répertoire" + ex), LogType.Error);
            }
        }


        /// <summary>
        /// Downloads as chunk.
        /// </summary>
        /// <returns></returns>
        private void Start(DownloadContentTask task)
        {
            try
            {
                BackgroundJob.Enqueue(() => ExecuteStart(task.Origin.Link, task.Destination));

                SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>($"Téléchargement en cours... {task.Origin}"), LogType.Info);
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>($"Une erreur c'est produite"), LogType.Error);
            }

        }

        [DisableConcurrentExecution(150 * 60)]
        [Queue("m3u-download")]
        public static void ExecuteStart(string from, string to)
        {
            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(from, to);
        }

        private void OnFailed(DownloadContentTask task, Exception ex)
        {
            SimpleLogger.AddLog(new DownloadResultLog<ContentDownloaderClient>
            {
                HasSucceeded = false,
                Origin = task.Origin.Link,
                Destination = task.Destination,
                Exception = ex ?? new ArgumentNullException(nameof(ex)),
                EndedTimeJob = DateTime.Now,
                StartedTimeJob = StartedTime,
            });

            task.IsFinish = true;
            task.HasSucceeded = false;

            if (task.Content is Data.Movie movie)
            {
                ContentDbContext.Movies.UpdateEntity(movie);
                movie.SetDirty();
            }

            task.Origin.IsDownloading = false;
            task.Origin.IsDownloaded = false;

            ContentDbContext.Movies.SaveChanges();

        }

        private void OnSucceeded(DownloadContentTask task)
        {
            SimpleLogger.AddLog(new DownloadResultLog<ContentDownloaderClient>
            {
                HasSucceeded = true,
                Destination = task.Destination,
                EndedTimeJob = DateTime.Now,
                StartedTimeJob = StartedTime
            });

            task.IsFinish = true;

            if (task.Content is Movie movie)
            {
                ContentDbContext.Movies.UpdateEntity(movie);
                movie.SetDirty();
            }

            task.Origin.IsDownloading = false;
            task.Origin.IsDownloaded = true;

            ContentDbContext.Movies.SaveChanges();
        }
    }

    public class DownloadContentTask
    {
        public string Destination { get; set; }
        public DirectLinkDownload Origin { get; set; }
        public Content Content { get; set; }
        public bool IsFinish { get; set; } = false;
        public bool HasSucceeded { get; set; } = true;
    }
}

