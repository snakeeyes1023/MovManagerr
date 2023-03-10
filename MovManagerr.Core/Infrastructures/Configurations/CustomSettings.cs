using Microsoft.AspNetCore.Components.Forms;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations.ContentPreferences;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Configurations
{
    [Table("Settings")]
    public class CustomSettings : Entity
    {
        public CustomSettings()
        {
            ContentPreferences = new List<ContentPreference>
            {
                new MoviePreference()
            };

            PlexConfiguration = new PlexConfiguration();
            TranscodeConfiguration = new TranscodeConfiguration();
        }

        public virtual IList<ContentPreference> ContentPreferences { get; set; }

        public virtual IList<string> Langs { get; set; } = new List<string>();

        public virtual IList<M3ULink> Links { get; private set; } = new List<M3ULink>();

        public virtual bool UseOpenAI { get; set; }
        
        public virtual string OpenAIApiKey { get; set; }

        public virtual PreferenceDownload DownloadHours { get; private set; }

        public virtual PlexConfiguration PlexConfiguration { get; private set; }

        public virtual TranscodeConfiguration TranscodeConfiguration { get; private set; }

        public T GetContentPreference<T>() where T : ContentPreference
        {
            foreach (var preference in ContentPreferences)
            {
                if (preference is T contentPreference)
                {
                    return contentPreference;
                }
            }

            throw new InvalidDataException($"Impossible de trouver une configuration pour le type de contenue : {typeof(T).Name}");
        }

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
