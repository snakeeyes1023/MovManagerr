using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations.ContentPreferences;
using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MovManagerr.Core.Infrastructures.Configurations
{
    [Table("Settings")]
    public class CustomSettings : Entity
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

        public ContentPreference<T> GetContentPreference<T>() where T : Content
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

    public class TranscodeConfiguration
    {
        public decimal MaximalBitrate { get; set; }
        public decimal MaximalGb { get; set; }
        public string FFmpegString { get; set; }

        public TranscodeConfiguration()
        {
            MaximalBitrate = 10000;
            MaximalGb = 8;
            FFmpegString = "-i \"{INTPUTFILE}\" -c:v libx264 -preset veryfast -crf 23 -c:a copy -c:s copy -maxrate 8000k -bufsize 2000k -movflags +faststart \"{OUTPUTFILE}\"";
        }

        public string GetTranscodeFFmpegString(string inputFile, string outputFile)
        {
            return FFmpegString
                .Replace("{INTPUTFILE}", inputFile)
                .Replace("{OUTPUTFILE}", outputFile)
                .Replace("{MAXIMALBITRATE}", MaximalBitrate.ToString())
                .Replace("{MAXIMALGB}", MaximalGb.ToString());
        }

        public bool IsTranscodeEnabled()
        {
            return MaximalBitrate > 0 || MaximalGb > 0;
        }

        public bool IsTranscodeRequired(DownloadedContent content)
        {
            if (!IsTranscodeEnabled())
            {
                return false;
            }
            return content.FileSizeAsGb > MaximalGb || content.VideoInfo.Bitrate > MaximalBitrate;
        }
    }
}
