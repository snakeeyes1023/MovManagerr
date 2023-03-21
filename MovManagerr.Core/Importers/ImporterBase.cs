using Hangfire;
using MovManagerr.Core.Infrastructures.Dbs;

namespace MovManagerr.Core.Importers
{
    public abstract class ImporterBase
    {
        protected readonly DbContext _dbContext;

        public ImporterBase(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Schedule_Import()
        {
            BackgroundJob.Enqueue(() => Import(CancellationToken.None));
        }

        [Queue("sync-task")]
        public abstract Task Import(CancellationToken cancellationToken);
    }
}