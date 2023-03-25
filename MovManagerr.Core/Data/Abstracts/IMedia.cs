namespace MovManagerr.Core.Data.Abstracts
{
    public interface IMedia
    {
        bool IsDownloaded { get; }
        List<DownloadedContent> DownloadedContents { get; }
        int NbFiles { get; }
    }
}