using MovManagerr.Tmdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Config
{
    public sealed class Preferences
    {
        public static Preferences Instance { get { return Nested.instance; } }


        private readonly string _PreferenceFolder;

        #region Path
        public string TempPath { get { return Path.Combine(_PreferenceFolder, "Temp"); } }
        public string MovieFolder { get { return Configs["MoviePath"]; } }
        public string SerieFolder { get { return Configs["SeriePath"]; } }


        public string[] Langs { get { return Configs["Lang"].Split(','); } }

        public string[] Links { get; private set; }

        public Dictionary<string, string> Configs { get; private set; }
        #endregion


        private Preferences()
        {
            // L'orde est important
            _PreferenceFolder = Path.Combine(Environment.CurrentDirectory, "Config");

            ReadConfig();
            ReadLinks();
        }

        public static TmdbClientService GetTmdbInstance()
        {
            return new TmdbClientService(new MovManagerr.Tmdb.Config.TmdbConfig()
            {
                ApiKey = "b41cf1ce06b0bf7826e538951a966a49",
                Session = "82e668e10bbfbf820bf326fcca5a487cc4f44652",
                Language = "fr",
                UseSsl = false,
                Url = "http://api.themoviedb.org"
            });
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
                            Configs.Add(split[0], split[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Impossible de lire le fichier de configuration", ex);
            }
            
            ValidateConfiguration();
        }

        public void ReadLinks()
        {
            Links = System.IO.File.ReadAllLines(Path.Combine(_PreferenceFolder, "links.conf"));

            if (Links == null || !Links.Where(x => !string.IsNullOrWhiteSpace(x)).Any())
            {
                throw new InvalidOperationException("Aucun lien dans le fichier links.conf");
            }
        }

        private void ValidateConfiguration()
        {
            if (Configs != null && Configs.Any())
            {
                if (!Configs.Any(x => x.Key == "MoviePath"))
                {
                    throw new InvalidOperationException("Le chemin vers les films n'est pas configuré");
                }

                if (!Configs.Any(x => x.Key == "SeriePath"))
                {
                    throw new InvalidOperationException("Le chemin vers les séries n'est pas configuré");
                }

                if (!Configs.Any(x => x.Key == "Lang"))
                {
                    throw new InvalidOperationException("La langue n'est pas configurée");
                }
            }
            else
            {
                throw new InvalidOperationException("Les configurations dans le fichier pref.conf sont invalides");
            }
        }



        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly Preferences instance = new Preferences();
        }
    }
}
