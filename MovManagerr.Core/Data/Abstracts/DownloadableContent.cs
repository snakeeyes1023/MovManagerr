namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class DownloadableContent : IEquatable<DownloadableContent>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is downloaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is downloaded; otherwise, <c>false</c>.
        /// </value>
        /// 
        public virtual bool IsDownloaded { get; set; }
        public virtual bool IsDownloading { get; set; }
        public virtual string Source { get; set; }
        public virtual string CodecVideo { get; set; }
        public virtual string InfoAudio { get; set; }
        public virtual string Size { get; set; }
        public virtual string Langues { get; set; }
        public virtual string Quality { get; set; }

        public abstract bool Equals(DownloadableContent? other);

        public abstract void Download(IServiceProvider serviceProvider, Content content);
    }
}
