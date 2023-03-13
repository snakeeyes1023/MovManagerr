using MovManagerr.Core.Data.Abstracts;
using Xabe.FFmpeg;

namespace MovManagerr.Core.Infrastructures.Configurations
{
    public class TranscodeConfiguration
    {
        public decimal MaximalBitrate { get; set; }
        public decimal MaximalGb { get; set; }
        public string FFmpegString { get; set; }

        public string DirectoryPath { get; set; }

        public TranscodeConfiguration()
        {
            MaximalBitrate = 8000;
            MaximalGb = 8;
            FFmpegString = "-i \"{INTPUTFILE}\" -crf 18 -map 0 -acodec copy -scodec copy -c:v libx264 -b:v {MAXIMALBITRATE}k -maxrate {MAXIMALBITRATE}k -bufsize {MAXIMALBITRATE}k -threads 0 -preset veryfast \"{OUTPUTFILE}\"";
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
            return content.FileSizeAsGb > MaximalGb || (content.OverallInfo.BitrateInMbs * 100) > MaximalBitrate;
        }

        public bool VerifyFFmpegInstallation()
        {
            try
            {
                var conversionResult = FFmpeg.Conversions.New().Start("-version").Wait(100);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message.Contains("PATH", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
