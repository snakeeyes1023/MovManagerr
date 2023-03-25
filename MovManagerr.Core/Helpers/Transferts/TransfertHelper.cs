using Hangfire;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.TrackedTasks.Generals;
using System.Net;
using TMDbLib.Objects.Changes;

namespace MovManagerr.Core.Helpers.Transferts
{

    public class TransfertHelper
    {
        private readonly TransfertContext transfertContext;

        private TransfertHelper()
        {
            transfertContext = new TransfertContext();
            transfertContext.TriggerPlexScan = Preferences.Instance.Settings.PlexConfiguration.TriggerScanOnMoved;
        }

        public static TransfertHelper New()
        {
            return new TransfertHelper();
        }

        public TransfertHelper To(string destinationPath)
        {
            transfertContext.Destination = destinationPath;
            return this;
        }
        public TransfertHelper From(string origin)
        {
            transfertContext.Origin = origin;
            return this;
        }

        public TransfertHelper Replace(bool replace)
        {
            transfertContext.Replace = replace;
            return this;
        }

        public TransfertHelper DeleteOrigin(bool enable = true)
        {
            transfertContext.DeleteOrigin = enable;
            return this;
        }

        public TransfertHelper ContentType(ContentType contentType)
        {
            transfertContext.ContentType = contentType;
            return this;
        }

        public TransfertHelper TriggerPlexScan(bool enable)
        {
            transfertContext.TriggerPlexScan = enable;
            return this;
        }

        public void EnqueueMove()
        {
            string jobId = BackgroundJob.Enqueue<TransfertWithDbContext>(x => x.MoveFileWithProgress(transfertContext, CancellationToken.None, null));
            GlobalTrackedTask.AddTransfertJobIfNotExist(jobId, transfertContext.Origin, transfertContext.Destination);
        }
    }
}
