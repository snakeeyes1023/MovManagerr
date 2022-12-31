using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Downloader;
using M3USync.Config;
using M3USync.Models;
using M3USync.Models.Intefaces;

namespace M3USync.Http
{
    public sealed class ContentDownloader
    {
        public static ContentDownloader Instance { get { return Nested.instance; } }

        private bool CanHandle { get; set; }
        public Task CurrentTask { get; set; }

        public List<Action> PendingTasks { get; set; } = new List<Action>();

        private ContentDownloader()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            CanHandle = true;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
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

                Directory.CreateDirectory(path);

                AppendTask(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            CanHandle = false;

            Thread.Sleep(1000);

            WebClient myWebClient = new WebClient();
            myWebClient.DownloadProgressChanged += Client_DownloadProgressChanged;
            myWebClient.DownloadFile(url, destinationPath);
            CanHandle = true;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Téléchargement terminé : {destinationPath}");
            Console.ResetColor();

            // Vérifie s'il y a des tâches en attente et exécute la prochaine tâche si possible
            CheckForPendingTasks();
        }

        private void Client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            // Calcul de la pourcentage de téléchargement
            
            double percentComplete = (double)e.BytesReceived / e.TotalBytesToReceive * 100;
            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = 32;
            Console.Write("]");
            Console.CursorLeft = 1;
            int progressBlockCount = (int)(percentComplete / 2.0);
            for (int i = 0; i < progressBlockCount; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(" ");
            }
            for (int i = progressBlockCount; i < 50; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.CursorLeft = 53;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($"{percentComplete,3:N0}%");

        }

        private void CheckForPendingTasks()
        {
            if (CanHandle && PendingTasks.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Début du téléchargement suivant...");
                Console.ResetColor();

                // Exécute la prochaine tâche en attente
                PendingTasks[0]();
                // Supprime la tâche de la liste des tâches en attente
                PendingTasks.RemoveAt(0);
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

