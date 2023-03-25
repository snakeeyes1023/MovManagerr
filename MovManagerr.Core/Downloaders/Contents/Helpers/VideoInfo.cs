namespace MovManagerr.Core.Downloaders.Contents.Helpers
{
    public class VideoInfo
    {
        public string Codec { get; private set; }
        public int Width { get; private set; }
        public int Heigth { get; private set; }
        public double FrameRate { get; private set; }
        public string FrameRateMode { get; private set; }
        public string ScanType { get; private set; }
        public TimeSpan Duration { get; private set; }
        public int Bitrate { get; private set; }
        public string AspectRatioMode { get; private set; }
        public double AspectRatio { get; private set; }

        public VideoInfo(MediaInfo mi)
        {
            Codec = mi.Get(StreamKind.Video, 0, "Format");

            int width;
            if (int.TryParse(mi.Get(StreamKind.Video, 0, "Width"), out width))
            {
                Width = width;
            }

            int height;
            if (int.TryParse(mi.Get(StreamKind.Video, 0, "Height"), out height))
            {
                Heigth = height;
            }

            int duration;
            if (int.TryParse(mi.Get(StreamKind.Video, 0, "Duration"), out duration))
            {
                Duration = TimeSpan.FromMilliseconds(duration);
            }

            int bitrate;
            if (int.TryParse(mi.Get(StreamKind.Video, 0, "BitRate"), out bitrate))
            {
                Bitrate = bitrate;
            }

            AspectRatioMode = mi.Get(StreamKind.Video, 0, "AspectRatio/String"); //as formatted string

            double aspectRatio;
            if (double.TryParse(mi.Get(StreamKind.Video, 0, "AspectRatio"), out aspectRatio))
            {
                AspectRatio = aspectRatio;
            }

            double frameRate;
            if (double.TryParse(mi.Get(StreamKind.Video, 0, "FrameRate"), out frameRate))
            {
                FrameRate = frameRate;
            }

            FrameRateMode = mi.Get(StreamKind.Video, 0, "FrameRate_Mode");
            ScanType = mi.Get(StreamKind.Video, 0, "ScanType");
        }

        public VideoInfo()
        {
            //pour liteDB
        }
    }
}

