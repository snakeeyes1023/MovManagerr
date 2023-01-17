using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using System.Net;
using System.Net.NetworkInformation;

namespace MovManagerr.Core.Downloaders.Contents
{
    public sealed class ContentDownloaderClient
    {
        public static ContentDownloaderClient Instance { get { return Nested.instance; } }

        private bool CanHandle { get; set; }
        public Task CurrentTask { get; set; }
        public DateTime StartedTime { get; set; }
        private int Attempt { get; set; }
        public TimeSpan DelayBetweenAttempt { get; set; }

        public List<DownloadContentTask> AllTasks { get; set; } = new List<DownloadContentTask>();
        private Content? ContentInDownload { get; set; }


        private ContentDownloaderClient(int attempt = 3)
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            CanHandle = true;
            Attempt = attempt;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
            SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>($"Changement d'état de réseaux (Est connecté : {e.IsAvailable})"), LogType.Info);

            if (e.IsAvailable)
            {
                // Si la connexion réseau est de nouveau disponible, vérifie s'il y a des tâches en attente
                // et exécute la prochaine tâche si possible
                CheckForPendingTasks();
            }
        }
        
        public void Download(Content content, DirectLinkDownload link)
        {
            try
            {
                var path = content.GetDirectoryPath();

                content
                    .GetDirectoryManager()
                    .CreateDirectory(path);

                string fullPath = Path.Combine(content.GetDirectoryManager()._BasePath, path, link.GetFilename());


                var downloadTask = new DownloadContentTask()
                {
                    Content = content,
                    Destination = fullPath,
                    IsFinish = false,
                    Origin = link.Link
                };

                AppendTask(downloadTask);
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>("Impossible de créer le répertoire" + ex), LogType.Error);
                Thread.Sleep(2000);
            }
        }

        private void AppendTask(DownloadContentTask task)
        {
            AllTasks.Add(task);

            // Vérifie s'il y a des tâches en attente et exécute la prochaine tâche si possible
            CheckForPendingTasks();
        }

        public bool IsOnWaitingList(Content content)
        {
            return AllTasks.Any(x => x.Content == content && !x.IsFinish);
        }

        public bool IsDownloading(Content content)
        {
            return ContentInDownload == content;
        }


        /// <summary>
        /// Downloads as chunk.
        /// </summary>
        /// <returns></returns>
        public void Start(DownloadContentTask task)
        {
            try
            {
                ContentInDownload = task.Content;

                WaitWhileNotInOperationHour();

                SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>($"Téléchargement en cours... {task.Origin}"), LogType.Info);

                CanHandle = false;

                Thread.Sleep(1000);

                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(task.Origin, task.Destination);

                OnSucceeded(task);
            }
            catch (Exception ex)
            {
                ++Attempt;

                OnFailed(task, ex);

                if (Attempt > 3)
                {
                    SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>("Téléchargement abandonné"), LogType.Error);
                    Attempt = 0;
                }
                else
                {
                    SimpleLogger.AddLog(new BasicLog<ContentDownloaderClient>("Nouvelle tentative dans " + DelayBetweenAttempt.TotalMinutes + " minutes"), LogType.Info);
                    Start(task);
                }
            }
            finally
            {
                ContentInDownload = null;
                CanHandle = true;
                Attempt = 0;
                CheckForPendingTasks();
            }
        }

        private void OnFailed(DownloadContentTask task, Exception ex)
        {
            SimpleLogger.AddLog(new DownloadResultLog<ContentDownloaderClient>
            {
                HasSucceeded = false,
                Origin = task.Origin,
                Destination = task.Destination,
                Exception = ex ?? new ArgumentNullException(nameof(ex)),
                EndedTimeJob = DateTime.Now,
                StartedTimeJob = StartedTime,
                Attempts = Attempt
            });

            task.IsFinish = true;
            task.HasSucceeded = false;
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

        }

        private void CheckForPendingTasks()
        {
            if (CanHandle && AllTasks != null && AllTasks.Any())
            {
                StartedTime = DateTime.Now;

                CurrentTask = Task.Run(() =>
                {
                    if (AllTasks.FirstOrDefault(x => !x.IsFinish) is DownloadContentTask task)
                    {
                        Start(task);
                    }
                });
            }
        }

        private void WaitWhileNotInOperationHour()
        {
            while (!Preferences.Instance.DownloadHours.CanDownload())
            {
                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ContentDownloaderClient instance = new ContentDownloaderClient();
        }
    }

    public class DownloadContentTask
    {
        public string Destination { get; set; }
        public string Origin { get; set; }
        public Content Content { get; set; }
        public bool IsFinish { get; set; } = false;
        public bool HasSucceeded { get; set; } = true;
    }
}

