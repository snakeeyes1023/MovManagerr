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
    public class Syncroniser : Command
    {
        private readonly Preferences _preferences;

        private readonly List<IReader> _readers;

        //private readonly DownloaderGlobalUI _ui;

        public Syncroniser() : base("Syncroniser les fichier M3U avec la base de donnée", "La synchronisation est terminée")
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
                Console.WriteLine($"Lecture du lien {link} ...");

                var tempPath = Path.GetTempPath();

                IM3uDownloader downloader = new M3uDownloaderClient(link, _readers);

                downloader.Start(tempPath).Wait();

                Console.WriteLine($"Fin de la lecture du lien {link}");
            }

            Console.WriteLine("Téléchargement terminé");

            Console.WriteLine("Synchronisation des données dans la base de données");

            foreach (var reader in _readers)
            {
                reader.SyncInDatabase();
                reader.Dispose();
            }
        }
    }
}
