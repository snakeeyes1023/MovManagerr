using Microsoft.Extensions.Options;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations.ContentPreferences;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using System.Net;

namespace MovManagerr.Core.Infrastructures.Configurations
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

        public readonly string _AppData;

        public readonly string _DbPath;
        public readonly string _HangFireDbPath;
        public CustomSettings Settings { get; private set; }

        #endregion

        private Preferences()
        {
            // L'orde est important
            _AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Movmanagerr");
            _PreferenceFolder = Path.Combine(_AppData, "preferences");
            _DbPath = "Filename=" + Path.Combine(_AppData, "movmanagerr.db") + ";Connection=shared";
            _HangFireDbPath = Path.Combine(_AppData, "hangfire.db");
            try
            {
                InitaliseAppDataFolders();
                Settings = GetOrCreateSettings();
            }
            catch (Exception)
            {
                
            }
        }

        private CustomSettings GetOrCreateSettings()
        {
            ContentDbContext contentDbContext = new ContentDbContext(_DbPath);

            var results = contentDbContext.Settings.ToList();

            if (results != null && results.Any())
            {
                return results.FirstOrDefault()!;
            }
            else
            {
                CustomSettings setting = new CustomSettings();

                contentDbContext.Settings.Add(setting);
                contentDbContext.Settings.SaveChanges();

                return setting;
            }
        }

        public void ResetFactoryConfiguration()
        {
            CustomSettings setting = new CustomSettings();
            setting._id = GetOrCreateSettings()._id;
            Settings= setting;
            SaveSettings();

            SimpleLogger.AddLog("Remise des configurations d'usine", LogType.Warning);
        }

        public void SaveSettings()
        {
            ContentDbContext contentDbContext = new ContentDbContext(_DbPath);
            contentDbContext.Settings.TrackEntity(Settings);
            Settings.SetDirty();
            
            contentDbContext.Settings.SaveChanges();

            SimpleLogger.AddLog("Les configurations ont été mise à jour", LogType.Info);
        }

        public void ReloadSettings()
        {
            ContentDbContext contentDbContext = new ContentDbContext(_DbPath);

            var results = contentDbContext.Settings.ToList();

            if (results != null && results.Any())
            {
                Settings = results.FirstOrDefault()!;
            }
        }
        private void InitaliseAppDataFolders()
        {
            Directory.CreateDirectory(_AppData);
            Directory.CreateDirectory(_PreferenceFolder);
        }

        public static TmdbClientService GetTmdbInstance()
        {
            var tmdbConfig = new MovManagerr.Tmdb.Config.TmdbConfig()
            {
                ApiKey = "b41cf1ce06b0bf7826e538951a966a49",
                Session = "82e668e10bbfbf820bf326fcca5a487cc4f44652",
                Language = "fr",
                UseSsl = false,
                Url = "http://api.themoviedb.org"
            };

            IOptions<MovManagerr.Tmdb.Config.TmdbConfig> tmdb = Options.Create(tmdbConfig);

            return new TmdbClientService(tmdb);
        }

        public bool IsValid()
        {
            foreach (var item in Settings.ContentPreferences)
            {
                if (item is MoviePreference moviePreference && !string.IsNullOrWhiteSpace(moviePreference.BasePath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
