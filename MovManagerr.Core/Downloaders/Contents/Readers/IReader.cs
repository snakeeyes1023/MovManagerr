using MovManagerr.Core.Downloaders.M3U;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public interface IReader : IDisposable
    {
        event Action OnContentProceeded;

        void Read(MediaM3u mediaInfo);

        void SyncInDatabase();
    }
}
