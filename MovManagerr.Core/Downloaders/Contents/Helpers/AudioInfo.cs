namespace MovManagerr.Core.Downloaders.Contents.Helpers
{
    public class AudioInfo
    {
        public string Codec { get; private set; }
        public string CompressionMode { get; private set; }
        public string ChannelPositions { get; private set; }
        public TimeSpan Duration { get; private set; }
        public int Bitrate { get; private set; }
        public string BitrateMode { get; private set; }
        public int SamplingRate { get; private set; }

        public AudioInfo(MediaInfo mi)
        {
            Codec = mi.Get(StreamKind.Audio, 0, "Format");
            int.TryParse(mi.Get(StreamKind.Audio, 0, "Duration"), out int duration);
            Duration = TimeSpan.FromMilliseconds(duration);
            int.TryParse(mi.Get(StreamKind.Audio, 0, "BitRate"), out int bitrate);
            Bitrate = bitrate;
            BitrateMode = mi.Get(StreamKind.Audio, 0, "BitRate_Mode");
            CompressionMode = mi.Get(StreamKind.Audio, 0, "Compression_Mode");
            ChannelPositions = mi.Get(StreamKind.Audio, 0, "ChannelPositions");
            int.TryParse(mi.Get(StreamKind.Audio, 0, "SamplingRate"), out int samplingRate);
            SamplingRate = samplingRate;
        }

        public AudioInfo()
        {
            //pour liteDB
        }
    }
}
