using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using MovManagerr.Tmdb;

namespace MovManagerr.Web.Controllers
{
    public class MovieTaskController : Controller
    {
        private readonly TmdbClientService _tmdbClient;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public MovieTaskController(TmdbClientService tmdbService, IBackgroundJobClient backgroundJobs)
        {
            _tmdbClient = tmdbService;
            _backgroundJobClient = backgroundJobs;
        }
        
        [HttpPost]
        public JsonResult DeleteFavoriteMovies()
        {
            _backgroundJobClient.Enqueue<Tmdb.Service.FavoriteService>(x => x.FlushFavoriteMoviesAsync());
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult LikeNewMovie()
        {
            _backgroundJobClient.Enqueue<Tmdb.Service.FavoriteService>(x => x.LikeNewMovieAsync(2));
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult SyncFolderWithTmdb()
        {
            _backgroundJobClient.Enqueue<Explorer.Services.ContentServices>(x => x.SyncMovieListByFolderAsync());
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteBadMovies()
        {
            _backgroundJobClient.Enqueue<Explorer.Services.ContentServices>(x => x.DeleteBadMovie());
            return Json(new { success = true });
        }
    }
}
