using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.NewFolder;
using MovManagerr.Core.Infrastructures.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Services
{
    public class TranscodeService
    {
        private readonly TranscodeConfiguration _transcodeConfiguration;

        public TranscodeService() 
        {
            _transcodeConfiguration = MovManagerr.Core.Infrastructures.Configurations.Preferences.Instance.Settings.TranscodeConfiguration;
        }

        public bool IsTranscodeRequired(string moviePath)
        {
            var movieFile = new DownloadedContent(moviePath);
            movieFile.LoadMediaInfo();
            return _transcodeConfiguration.IsTranscodeRequired(movieFile);
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
