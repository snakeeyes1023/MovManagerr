using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.Transcode;
using MovManagerr.Core.Helpers.Transferts;
using MovManagerr.Core.Infrastructures.DataAccess;
using MovManagerr.Core.Services.Movies;

namespace MovManagerr.Core.Tasks
{

    [Flags]
    public enum ImportMode
    {
        None = 1,
        Move = 2,
        TranscodeAndMove = 4,
    }

    
    public class ImportContentService
    {
        private readonly DbContext _dbContext;

        public ImportContentService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public void ImportMovie(Movie movie, string originPath, ImportMode importMode)
        {
            string destinationPath = movie.GetFullPath(Path.GetFileName(originPath));
    

            if (movie.DownloadedContents.FirstOrDefault(x => x.FullPath == destinationPath) is DownloadedContent alreadyExist)
            {
                // un fichier du même nom existe déjà
                destinationPath = movie.GetFullPath(Path.GetFileNameWithoutExtension(originPath) + "_1" + Path.GetExtension(originPath));
            }

            movie.CreateAndScan(originPath);

            _dbContext.Movies.Update(movie);

            // if need importmode need to transcode
            if (importMode == ImportMode.TranscodeAndMove)
            {
                TranscodeHelper.New()
                    .From(originPath)
                    .To(destinationPath)
                    .ContentType(ContentType.Movie)
                    .EnqueueRun();
            }
            else
            {
                TransfertHelper.New()
                    .From(originPath)
                    .To(destinationPath)
                    .ContentType(ContentType.Movie)
                    .EnqueueMove();
            }
        }       
    }
}
