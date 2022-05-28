using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MovManagerr.Tmdb;
using MovManagerr.Web.Infrastructure;

namespace MovManagerr.Web.Controllers
{
    [ServiceFilter(typeof(AdminActionFilter))]
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
            _backgroundJobClient.Enqueue<Explorer.Services.MovieServices>(x => x.SyncMovieListByFolderAsync());
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteBadMovies()
        {
            _backgroundJobClient.Enqueue<Explorer.Services.MovieServices>(x => x.DeleteBadMovie());
            return Json(new { success = true });
        }
    }
}
