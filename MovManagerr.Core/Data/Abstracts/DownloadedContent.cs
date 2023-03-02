using MediaInfoLib;
using MovManagerr.Core.Downloaders.Contents.Helpers;
using MovManagerr.Core.Infrastructures.Loggers;
using System.IO;

namespace MovManagerr.Core.Data.Abstracts
{
    public class DownloadedContent
    {
        public DownloadedContent() { }
        public DownloadedContent(string fullPath, DownloadableContent? method = null)
        {
            FullPath = fullPath;
            Method = method;
            CreationDate = DateTime.Now;
        }
        public string FullPath { get; set; }
        public VideoInfo VideoInfo { get; set; }
        public AudioInfo AudioInfo { get; set; }
        public string FileSize { get; set; }
        public DownloadableContent? Method { get; protected set; }
        public DateTime CreationDate { get; protected set; }

        public void LoadMediaInfo(string fullPath = "")
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                fullPath = FullPath;
            }

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentNullException("Le chemin du fichié est null");
            }

            var mi = new MediaInfo();

            mi.Open(fullPath);
            VideoInfo = new VideoInfo(mi);
            AudioInfo = new AudioInfo(mi);
            FileSize = mi.Get(StreamKind.General, 0, "FileSize/String2");

            mi.Close();
        }

        //public string GetMediaAvailableParameter()
        //{
        //    var mi = new MediaInfo();
        //    string parameter = mi.Option("Info_Parameters");
        //    mi.Close();

        //    return parameter;
        //}

        //public string GetAllInfo()
        //{
        //    var mi = new MediaInfo();
        //    mi.Open(@"video path here");
        //    Console.WriteLine(mi.Inform());
        //    mi.Close();
        //}
    }


    public static class ContentDownloadedExtensions
    {
        public static decimal GetMaxBitrate(this Content content)
        {
            var maxBitrate = 0;

            if (content.DownloadedContents != null)
            {
                foreach (var downloaded in content.DownloadedContents)
                {
                    if (maxBitrate < (downloaded.VideoInfo?.Bitrate ?? 0))
                    {
                        maxBitrate = downloaded.VideoInfo?.Bitrate ?? 0;
                    }
                }
            }

            return decimal.Divide(maxBitrate, 1000000);
        }

        /// <summary>
        /// example return : 720p, 1080p
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static string GetBestQuality(this Content content)
        {
            int maxHeight = 0;

            if (content.DownloadedContents != null)
            {
                foreach (var downloaded in content.DownloadedContents)
                {
                    if (maxHeight < (downloaded.VideoInfo?.Heigth ?? 0))
                    {
                        maxHeight = downloaded.VideoInfo?.Heigth ?? 0;
                    }
                }
            }

            return $"{maxHeight}p";
        }

        public static bool HasAnyScannedFile(this Content content)
        {
            if (content.DownloadedContents != null)
            {
                foreach (var downloaded in content.DownloadedContents)
                {
                    if (downloaded.VideoInfo != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
