using M3USync.Downloaders.Contents.Readers;
using M3USync.Downloaders.M3U;
using M3USync.Infrastructures.Configurations;
using M3USync.Infrastructures.UIs;

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

                SimpleLogger.AddLog($"Lecture du lien {link} ...");

                M3uDownloaderClient downloader = new M3uDownloaderClient(link, _readers);

                downloader.Start(Path.GetTempPath()).Wait();

                SimpleLogger.AddLog($"Fin de la lecture du lien {link}");
            }

            SimpleLogger.AddLog("Synchronisation des données dans la base de données...");

            foreach (var reader in _readers)
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
