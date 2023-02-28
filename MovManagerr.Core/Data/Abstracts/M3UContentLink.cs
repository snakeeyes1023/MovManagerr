using Hangfire;
using MovManagerr.Core.Downloaders.Contents;

namespace MovManagerr.Core.Data.Abstracts
{
    public class M3UContentLink : DirectLinkDownload
    {
        public List<string> Tags { get; set; }

        public override void Download(IServiceProvider serviceProvider, Content content)
        {
            if (serviceProvider.GetService(typeof(ContentDownloaderClient)) is ContentDownloaderClient contentDownloadClient)
            {
                contentDownloadClient.Download(content, this);
            }
            else
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
        }

    }
}
