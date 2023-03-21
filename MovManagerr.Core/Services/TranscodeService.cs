using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;

namespace MovManagerr.Core.Services
{
    public class TranscodeService
    {
        private readonly TranscodeConfiguration _transcodeConfiguration;

        public TranscodeService()
        {
            _transcodeConfiguration = MovManagerr.Core.Infrastructures.Configurations.Preferences.Instance.Settings.TranscodeConfiguration;
        }

        public async Task<bool> IsTranscodeRequired(string moviePath)
        {
            var movieFile = new DownloadedContent(moviePath);

            try
            {
                await Task.Run(() => movieFile.LoadMediaInfo());

                return _transcodeConfiguration.IsTranscodeRequired(movieFile);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsTranscodeRequired(DownloadedContent content)
        {
            return _transcodeConfiguration.IsTranscodeRequired(content);
        }

        public void TranscodeContent(DownloadedContent content)
        {
            TranscodeHelper.New()
                .From(content.FullPath)
                .To(content.FullPath)
                .ReplaceDestination(true)
                .EnqueueRun();
        }
    }
}
