using MovManagerr.Core.Downloaders.Contents;
using MovManagerr.Core.Downloaders.Contents.Readers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Tasks.Backgrounds.ContentTasks
{
    public class SyncM3UFiles : EventedBackgroundService
    {
        private readonly List<IReader> _readers;
        private readonly Preferences _preferences;

        public SyncM3UFiles() : base("Synchronisation des fichiers M3U")
        {
            _preferences = Preferences.Instance;

            var movieReaders = new MovieReader();
            //var serieReaders = new SerieReader();

            _readers = new List<IReader>
            {
                movieReaders,
                //serieReaders
            };
        }

        protected override void PerformTask(CancellationToken cancellationToken)
        {
            foreach (var link in _preferences.Links)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SimpleLogger.AddLog($"Lecture du lien {link} ...");

                    M3uDownloaderClient downloader = new M3uDownloaderClient(link, _readers);

                    downloader.Start(Path.GetTempPath(), cancellationToken).Wait();

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
                        reader.SyncInDatabase();
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
