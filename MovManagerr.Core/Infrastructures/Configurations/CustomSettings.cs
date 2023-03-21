using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations.ContentPreferences;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovManagerr.Core.Infrastructures.Configurations
{
    [Table("Settings")]
    public class CustomSettings
    {
        public CustomSettings()
        {
            ContentPreferences = new List<IContentPreference>
            {
                new MoviePreference()
            };

            PlexConfiguration = new PlexConfiguration();
            TranscodeConfiguration = new TranscodeConfiguration();
        }

        public List<IContentPreference> ContentPreferences { get; set; }

        public ContentPreference<T> GetContentPreference<T>()
        {
            foreach (var preference in ContentPreferences)
            {
                if (preference is ContentPreference<T> contentPreference)
                {
                    return contentPreference;
                }
            }

            throw new InvalidDataException($"Impossible de trouver une configuration pour le type de contenue : {typeof(T).Name}");
        }

        public List<string> Langs { get; set; } = new List<string>();

        public List<M3ULink> Links { get; private set; } = new List<M3ULink>();

        public bool UseOpenAI { get; set; }
        public string OpenAIApiKey { get; set; }

        public PreferenceDownload DownloadHours { get; private set; }

        public PlexConfiguration PlexConfiguration { get; private set; }

        public TranscodeConfiguration TranscodeConfiguration { get; private set; }

        public void VerifyDriveAccessibility()
        {
            //check if the drive is accessible (if not, throw an exception) { movieFolder, serieFolder, tempPath }
            foreach (var path in ContentPreferences.Select(x => x.GetDirectoryManager()))
            {
                if (!path.VerifyAccessibilty())
                {
                    throw new InvalidOperationException(string.Format("Le chemin {0} n'existe pas ou n'est pas accessible", path._BasePath));
                }
            }
        }

        public bool IsPlexConfigure()
        {
            return PlexConfiguration != null && !string.IsNullOrWhiteSpace(PlexConfiguration.ApiKey);
        }
    }

    public class M3ULink
    {
        public string Link { get; set; }
    }
}
