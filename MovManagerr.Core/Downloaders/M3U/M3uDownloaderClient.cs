using Downloader;
using MovManagerr.Core.Downloaders.Contents.Readers;

namespace MovManagerr.Core.Downloaders.M3U
{
    public class M3uDownloaderClient
    {
        protected readonly string _destinationPath;
        protected readonly DownloadConfiguration _config;
        protected readonly string _url;

        public M3uChunkReadable Chunks { get; }

        public M3uDownloaderClient(string url, IEnumerable<IReader> readers)
        {
            Chunks = new M3uChunkReadable();

            _url = url;

            foreach (var reader in readers)
            {
                Chunks.OnContentFounded += reader.Read;
            }

            _config = new DownloadConfiguration()
            {
                // usually, hosts support max to 8000 bytes, default values is 8000
                BufferBlockSize = 10240,
                // file parts to download, default value is 1
                ChunkCount = 8,
                // download speed limited to 2MB/s, default values is zero or unlimited
                MaximumBytesPerSecond = 1024 * 1024 * 2,
                // the maximum number of times to fail
                MaxTryAgainOnFailover = 5,
                // download parts of file as parallel or not. Default value is false
                ParallelDownload = true,
                // number of parallel downloads. The default value is the same as the chunk count
                ParallelCount = 4,
                // timeout (millisecond) per stream block reader, default values is 1000
                Timeout = 1000,
                // set true if you want to download just a specific range of bytes of a large file
                RangeDownload = false,
                // floor offset of download range of a large file
                RangeLow = 0,
                // ceiling offset of download range of a large file
                RangeHigh = 0,
                // clear package chunks data when download completed with failure, default value is false
                ClearPackageOnCompletionWithFailure = true,
                // minimum size of chunking to download a file in multiple parts, default value is 512
                MinimumSizeOfChunking = 1024,
                // Before starting the download, reserve the storage space of the file as file size, default value is false
                ReserveStorageSpaceBeforeStartingDownload = true
            };
        }

        /// <summary>
        /// Downloads as chunk.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start(string destinationPath)
        {
            DirectoryInfo path = new DirectoryInfo(destinationPath);

            try
            {
                var downloadService = GetDownloadeService();

                await downloadService.DownloadFileTaskAsync(_url, path);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when [chunk download progress changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Downloader.DownloadProgressChangedEventArgs"/> instance containing the event data.</param>
        private void OnChunkDownloadProgressChanged(byte[] bytes)
        {
            Chunks.Add(bytes);
        }

        private DownloadService GetDownloadeService()
        {
            var downloadService = new DownloadService(_config);

            downloadService.ChunkDownloadProgressChanged += (sender, e) => OnChunkDownloadProgressChanged(e.ReceivedBytes);

            return downloadService;
        }
    }
}
