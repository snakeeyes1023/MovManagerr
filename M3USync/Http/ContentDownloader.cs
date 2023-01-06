using M3USync.Config;
using M3USync.Http.Models;
using M3USync.Models.Intefaces;
using System.Net;
using System.Net.NetworkInformation;

namespace M3USync.Http
{
    public sealed class ContentDownloader
    {
        public static ContentDownloader Instance { get { return Nested.instance; } }

        private bool CanHandle { get; set; }
        public Task CurrentTask { get; set; }
        public DateTime StartedTime { get; set; }
        private int Attempt { get; set; }
        public TimeSpan DelayBetweenAttempt { get; set; }

        public List<Action> PendingTasks { get; set; } = new List<Action>();

        public readonly List<ILog> Logs;

        public event Action<ILog> OnNewLogAdded;

        private ContentDownloader(int attempt = 3)
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            CanHandle = true;
            Attempt = attempt;
            Logs = new List<ILog>();
        }

        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
            AddLog(new BasicLog { Message = $"Changement d'état de réseaux (Est connecté : {e.IsAvailable})" });

            if (e.IsAvailable)
            {
                // Si la connexion réseau est de nouveau disponible, vérifie s'il y a des tâches en attente
                // et exécute la prochaine tâche si possible
                CheckForPendingTasks();
            }
        }

        public void Download(ISave content)
        {
            try
            {
                var path = content.GetDirectoryPath();

                content
                    .GetDirectoryManager()
                    .CreateDirectory(path);

                AppendTask(content);
            }
            catch (Exception ex)
            {
                AddLog(new BasicLog { Message = "Impossible de créer le répertoire" + ex });
                Thread.Sleep(2000);
            }
        }

        private void AppendTask(ISave content)
        {
            PendingTasks.Add(() =>
            {
                CurrentTask = (Task.Run(() =>
                {
                    Start(content.Url, content.GetFullPath());
                }));
            });

            // Vérifie s'il y a des tâches en attente et exécute la prochaine tâche si possible
            CheckForPendingTasks();
        }


        /// <summary>
        /// Downloads as chunk.
        /// </summary>
        /// <returns></returns>
        public void Start(string url, string destinationPath)
        {
            try
            {
                WaitWhileNotInOperationHour();

                AddLog(new BasicLog() { Message = $"Téléchargement en cours... {url}" });

                CanHandle = false;

                Thread.Sleep(1000);

                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(url, destinationPath);

                OnSucceeded(destinationPath);
            }
            catch (Exception ex)
            {
                ++Attempt;

                OnFailed(url, destinationPath, ex);

                if (Attempt > 3)
                {
                    AddLog(new BasicLog { Message = "Téléchargement abandonné" });
                    Attempt = 0;
                }
                else
                {
                    AddLog(new BasicLog { Message = "Nouvelle tentative dans " + DelayBetweenAttempt.TotalMinutes + " minutes" });
                    Start(url, destinationPath);
                }
            }
            finally
            {
                CanHandle = true;
                Attempt = 0;
                CheckForPendingTasks();
            }
        }

        private void OnFailed(string url, string destinationPath, Exception ex)
        {
            AddLog(new DownloadResult
            {
                HasSucceeded = false,
                Origin = url,
                Destination = destinationPath,
                Exception = ex ?? new ArgumentNullException(nameof(ex)),
                EndedTimeJob = DateTime.Now,
                StartedTimeJob = StartedTime,
                Attempts = Attempt
            });
        }

        private void OnSucceeded(string destinationPath)
        {
            AddLog(new DownloadResult
            {
                HasSucceeded = true,
                Destination = destinationPath,
                EndedTimeJob = DateTime.Now,
                StartedTimeJob = StartedTime
            });
        }

        private void AddLog(ILog log)
        {
            Logs.Add(log);
            OnNewLogAdded?.Invoke(log);
        }

        private void CheckForPendingTasks()
        {
            if (CanHandle && PendingTasks.Any())
            {
                StartedTime = DateTime.Now;
                
                // Exécute la prochaine tâche en attente
                PendingTasks[0]();
                // Supprime la tâche de la liste des tâches en attente
                PendingTasks.RemoveAt(0);
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

            internal static readonly ContentDownloader instance = new ContentDownloader();
        }
    }
}

