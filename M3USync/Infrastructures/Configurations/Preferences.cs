using MovManagerr.Tmdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.General;

namespace M3USync.Infrastructures.Configurations
{
    public sealed class Preferences
    {
        private static Preferences _instance;

        public static Preferences Instance
        {
            get
            {
                _instance ??= new Preferences();
                return _instance;
            }
        }

        public static void RebuildInstance()
        {
            _instance = new Preferences();
        }

        #region Path

        public readonly string _PreferenceFolder;

        private DirectoryManager _movieManager;
        public DirectoryManager MovieManager
        {
            get
            {
                if (_movieManager == null)
                {
                    _movieManager = GetDirectoryManager("movie");
                }
                return _movieManager;
            }
        }


        private DirectoryManager _serieManager;
        public DirectoryManager SerieManager
        {
            get
            {
                if (_serieManager == null)
                {
                    _serieManager = GetDirectoryManager("serie");
                }
                return _serieManager;
            }
        }

        public string[] Langs { get { return Configs["lang"].Split(','); } }

        public string[] Links { get; private set; }

        public readonly string _DbPath;

        private Dictionary<string, string> Configs;

        public PreferenceDownload DownloadHours
        {
            get
            {
                return new PreferenceDownload(Configs["operation_hour"]);
            }
        }

        #endregion


        private Preferences()
        {
            // L'orde est important
            _PreferenceFolder = Path.Combine(Environment.CurrentDirectory, "Config");
            _DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "movmanagerr.db");

            ReadConfig();
            ReadLinks();
        }

        public static TmdbClientService GetTmdbInstance()
        {
            return default;
            //return new TmdbClientService(new MovManagerr.Tmdb.Config.TmdbConfig()
            //{
            //    ApiKey = "b41cf1ce06b0bf7826e538951a966a49",
            //    Session = "82e668e10bbfbf820bf326fcca5a487cc4f44652",
            //    Language = "fr",
            //    UseSsl = false,
            //    Url = "http://api.themoviedb.org"
            //});
        }

        public void ReadConfig()
        {
            Configs = new Dictionary<string, string>();

            try
            {
                var lines = File.ReadAllLines(Path.Combine(_PreferenceFolder, "pref.conf"));

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var split = line.Split('=');

                        if (split.Length > 1 && !string.IsNullOrWhiteSpace(split[0]) && !string.IsNullOrWhiteSpace(split[1]))
                        {
                            Configs.Add(split[0].ToLower(), split[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Impossible de lire le fichier de configuration", ex);
            }
        }

        public void ReadLinks()
        {
            Links = File.ReadAllLines(Path.Combine(_PreferenceFolder, "links.conf"));

            if (Links == null || !Links.Where(x => !string.IsNullOrWhiteSpace(x)).Any())
            {
                throw new InvalidOperationException("Aucun lien dans le fichier links.conf");
            }
        }

        public void ValidateConfiguration()
        {
            if (Configs != null && Configs.Any())
            {
                if (!Configs.Any(x => x.Key == "movie_path"))
                {
                    throw new InvalidOperationException("Le chemin vers les films n'est pas configuré");
                }

                if (!Configs.Any(x => x.Key == "serie_path"))
                {
                    throw new InvalidOperationException("Le chemin vers les séries n'est pas configuré");
                }

                if (!Configs.Any(x => x.Key == "lang"))
                {
                    throw new InvalidOperationException("La langue n'est pas configurée");
                }
                if (!DownloadHours.IsValid)
                {
                    throw new InvalidOperationException("Les heures de téléchargement ne sont pas ou mal configuré");
                }
            }
            else
            {
                throw new InvalidOperationException("Les configurations dans le fichier pref.conf sont invalides");
            }
        }

        public void VerifyDriveAccessibility()
        {
            //check if the drive is accessible (if not, throw an exception) { movieFolder, serieFolder, tempPath }
            foreach (var path in new DirectoryManager[] { MovieManager, SerieManager })
            {
                if (!path.VerifyAccessibilty())
                {
                    throw new InvalidOperationException(string.Format("Le chemin {0} n'existe pas ou n'est pas accessible", path._BasePath));
                }
            }
        }

        private DirectoryManager GetDirectoryManager(string searchManager)
        {
            string path = Configs.GetValueOrDefault(searchManager + "_path", string.Empty);
            if (string.IsNullOrEmpty(path))
            {
                return new DirectoryManager(_PreferenceFolder);
            }
            else
            {
                string server = Configs.GetValueOrDefault(searchManager + "_server", string.Empty);

                if (!string.IsNullOrEmpty(server))
                {
                    string user = Configs.GetValueOrDefault(searchManager + "_user", string.Empty);
                    string password = Configs.GetValueOrDefault(searchManager + "_pass", string.Empty);

                    NetworkCredential cred = new NetworkCredential(user, password);
                    return new DirectoryCredsManager(path, server, cred);
                }

                return new DirectoryManager(path);
            }
        }
    }
}
