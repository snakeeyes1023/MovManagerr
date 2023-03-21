namespace MovManagerr.Core.Data.Abstracts
{
    public interface IMedia
    {
        bool IsDownloaded { get; }
        List<DownloadedContent> Medias { get; }
        int NbFiles { get; }
    }
}