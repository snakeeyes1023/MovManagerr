using MediaInfoLib;
using MovManagerr.Core.Downloaders.Contents.Helpers;

namespace MovManagerr.Core.Data.Abstracts
{

    public enum DownloadedContentType
    {
        Movie,
        Episode
    }
    
    public class DownloadedContent
    {
        public DownloadedContent() { }
        public DownloadedContent(string fullPath, DownloadableContent? method = null)
        {
            FullPath = fullPath;
            Method = method;
            CreationDate = DateTime.Now;
        }

        public int MovieId { get; set; }
        public int EpidodeId { get; set; }
        
        public string FullPath { get; set; }
        public VideoInfo VideoInfo { get; set; }
        public AudioInfo AudioInfo { get; set; }
        public OverallInfo OverallInfo { get; set; }
        public string FileSize { get; set; }
        public decimal FileSizeAsGb { get; set; }

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
            OverallInfo = new OverallInfo(mi);
            
            FileSize = mi.Get(StreamKind.General, 0, "FileSize/String2");
            long.TryParse(mi.Get(0, 0, "FileSize"), out long fileSizeLong);
            FileSizeAsGb = Decimal.Divide(fileSizeLong, 1000000000);

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
        public static decimal GetMaxBitrate(this IMedia content)
        {
            decimal maxBitrate = 0;

            if (content.Medias != null)
            {
                foreach (var downloaded in content.Medias)
                {
                    var bitrate = downloaded.OverallInfo?.BitrateInMbs;

                    if (bitrate != null && bitrate.Value > maxBitrate)
                    {
                        maxBitrate = bitrate.Value;
                    }
                }
            }

            return maxBitrate;
        }

        /// <summary>
        /// example return : 720p, 1080p
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static string GetBestQuality(this IMedia content)
        {
            int maxHeight = 0;

            if (content.Medias != null)
            {
                foreach (var downloaded in content.Medias)
                {
                    if (maxHeight < (downloaded.VideoInfo?.Heigth ?? 0))
                    {
                        maxHeight = downloaded.VideoInfo?.Heigth ?? 0;
                    }
                }
            }

            return $"{maxHeight}p";
        }

        public static bool HasAnyScannedFile(this IMedia content)
        {
            if (content.Medias != null)
            {
                foreach (var downloaded in content.Medias)
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
