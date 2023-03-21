using MovManagerr.Core.Downloaders.Contents.Readers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Core.Tasks.Backgrounds.ContentTasks
{
    public class SyncM3UFiles : EventedBackgroundService
    {
        private readonly List<IReader> _readers;
        private readonly Preferences _preferences;

        public SyncM3UFiles(IContentDbContext contentDbContext) : base("Synchronisation des fichiers M3U")
        {
            _preferences = Preferences.Instance;

            _readers = new List<IReader>
            {
                new MovieReader(contentDbContext),
            };
        }

        protected override void PerformTask(CancellationToken cancellationToken)
        {
            foreach (var link in _preferences.Settings.Links)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SimpleLogger.AddLog($"Lecture du lien {link} ...");

                    M3uDownloaderClient downloader = new M3uDownloaderClient(link.Link, _readers);

                    var result = Task.Run(async () => await downloader.Start(cancellationToken)).Result;

                    SimpleLogger.AddLog($"Fin de la lecture du lien {link}");
                }
            }

            SimpleLogger.AddLog("Synchronisation des données dans la base de données...");

            foreach (var reader in _readers)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SimpleLogger.AddLog("Synchronisation des données | " + reader.GetType().Name + " ...");

                    try
                    {
                        reader.SaveChanges();
                        reader.Dispose();
                        SimpleLogger.AddLog("Synchronisation des données | " + reader.GetType().Name + " terminée");
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.AddLog(ex.Message);
                    }
                }
            }
        }
    }
}
