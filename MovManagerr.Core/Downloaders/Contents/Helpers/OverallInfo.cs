using MediaInfoLib;

namespace MovManagerr.Core.Downloaders.Contents.Helpers
{
    public class OverallInfo
    {
        public decimal BitrateInMbs { get; private set; }
        public string BitrateString { get; private set; }
        public long MinimumBitrate { get; private set; }
        public string MinimumBitrateString { get; private set; }
        public long NominalBitrate { get; private set; }
        public string NominalBitrateString { get; private set; }
        public long MaximumBitrate { get; private set; }
        public string MaximumBitrateString { get; private set; }

        public OverallInfo(MediaInfo mi)
        {
            long.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate"), out long bitrate);
            BitrateInMbs = decimal.Divide(bitrate, 1000000);
            BitrateString = mi.Get(StreamKind.General, 0, "OverallBitRate/String");
            long.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Minimum"), out long minimumBitrate);
            MinimumBitrate = minimumBitrate;
            MinimumBitrateString = mi.Get(StreamKind.General, 0, "OverallBitRate_Minimum/String");
            long.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Nominal"), out long nominalBitrate);
            NominalBitrate = nominalBitrate;
            NominalBitrateString = mi.Get(StreamKind.General, 0, "OverallBitRate_Nominal/String");
            long.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Maximum"), out long maximumBitrate);
            MaximumBitrate = maximumBitrate;
            MaximumBitrateString = mi.Get(StreamKind.General, 0, "OverallBitRate_Maximum/String");
        }
        public OverallInfo()
        {
            //pour liteDB
        }
    }
}
