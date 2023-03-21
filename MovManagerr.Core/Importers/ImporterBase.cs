using Hangfire;

namespace MovManagerr.Core.Importers
{
    public abstract class ImporterBase
    {
        protected readonly IContentDbContext _contentDbContext;

        public ImporterBase(IContentDbContext dbContext)
        {
            _contentDbContext = dbContext;
        }

        public void Schedule_Import()
        {
            BackgroundJob.Enqueue(() => Import(CancellationToken.None));
        }

        [Queue("sync-task")]
        public abstract Task Import(CancellationToken cancellationToken);
    }
}