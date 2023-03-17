using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.NewFolder;
using MovManagerr.Core.Helpers.Transferts;
using MovManagerr.Core.Infrastructures.Dbs;
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
        private readonly IContentDbContext _contentDbContext;
        private readonly IMovieService _movieService;
        private readonly MovManagerr.Core.Infrastructures.Configurations.Preferences _preference;

        public ImportContentService(IContentDbContext contentDbContext, IMovieService movieService)
        {
            _contentDbContext = contentDbContext;
            _movieService = movieService;
            _preference = MovManagerr.Core.Infrastructures.Configurations.Preferences.Instance;
        }


        public void ImportMovie(Movie movie, string originPath, ImportMode importMode)
        {
            string destinationPath = movie.GetFullPath(Path.GetFileName(originPath));
    

            if (movie.DownloadedContents.FirstOrDefault(x => x.FullPath == destinationPath) is DownloadedContent alreadyExist)
            {
                // un fichier du même nom existe déjà
                destinationPath = movie.GetFullPath(Path.GetFileNameWithoutExtension(originPath) + "_1" + Path.GetExtension(originPath));
            }

            DownloadedContent downloadedContent = _movieService.CreateDownloadedContent(movie, originPath, destinationPath);

            // if need importmode need to transcode
            if (importMode == ImportMode.TranscodeAndMove)
            {
                TranscodeHelper.New()
                    .From(originPath)
                    .To(destinationPath)
                    .EnqueueRun();
            }
            else
            {
                TransfertHelper.New()
                    .From(originPath)
                    .To(destinationPath)
                    .MoveFile();
            }
        }       
    }
}
