using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.DataAccess;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Core.Helpers.Transferts
{
    public class TransfertWithDbContext : BaseTransfertHelper
    {
        private readonly DbContext _dbContext;
        public TransfertWithDbContext(DbContext dbContext)
        {
            _dbContext = dbContext;
            base.OnTransfertEnded += UpdateDownloadedContentEmplacement;
        }

        private void UpdateDownloadedContentEmplacement(object? sender, TransfertContext transfertContext)
        {
            try
            {
                if (transfertContext.ContentType == ContentType.Movie)
                {
                    Data.Movie movie = _dbContext.Movies
                        .FindByFullPath(transfertContext.Origin);

                    if (movie != null)
                    {
                        DownloadedContent downloadedContent = movie.DownloadedContents.FirstOrDefault(x => x.FullPath == transfertContext.Origin)!;
                        downloadedContent.FullPath = transfertContext.Destination;
                        _dbContext.Movies.Update(movie);
                    }
                }
                else if (transfertContext.ContentType == ContentType.TvShow)
                {
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.AddLog($"Erreur lors de la mise à jour de la base de données pour le fichier {transfertContext.Origin} : {ex.Message}", LogType.Error);
            }
        }

        public override void Dispose()
        {
            base.OnTransfertEnded -= UpdateDownloadedContentEmplacement;
        }
    }
}
