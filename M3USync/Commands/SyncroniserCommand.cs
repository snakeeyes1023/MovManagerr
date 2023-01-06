using M3USync.Config;
using M3USync.Http;
using M3USync.Readers;
using M3USync.UIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands
{
    public class SyncroniserCommand : Command
    {
        private readonly Preferences _preferences;

        private readonly List<IReader> _readers;

        //private readonly DownloaderGlobalUI _ui;

        public SyncroniserCommand() : base("Syncroniser les fichier M3U avec la base de donnée")
        {
            _preferences = Preferences.Instance;

            var movieReaders = new MovieReader();
            var serieReaders = new SerieReader();

            _readers = new List<IReader>
            {
                movieReaders,
                serieReaders
            };

            //_ui = new DownloaderGlobalUI(_readers);
        }


        protected override void Start()
        {
            foreach (var link in _preferences.Links)
            {

                AwesomeConsole.WriteInfo($"Lecture du lien {link} ...");

                var tempPath = Path.GetTempPath();

                IM3uDownloader downloader = new M3uDownloaderClient(link, _readers);

                downloader.Start(tempPath).Wait();

                AwesomeConsole.WriteInfo($"Fin de la lecture du lien {link}");
            }

            AwesomeConsole.WriteInfo("Synchronisation des données dans la base de données...");

            foreach (var reader in _readers)
            {
                AwesomeConsole.WriteInfo("Synchronisation des données | " + reader.GetType().Name + " ...");

                try
                {
                    reader.SyncInDatabase();
                    reader.Dispose();
                    AwesomeConsole.WriteInfo("Synchronisation des données | " + reader.GetType().Name + " terminée");
                }
                catch (Exception ex)
                {
                    AwesomeConsole.WriteError(ex.Message);
                }
            }
        }
    }
}
