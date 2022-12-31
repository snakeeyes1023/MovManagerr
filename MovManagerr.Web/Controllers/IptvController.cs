using M3USync.Commands;
using M3USync.Models;
using Microsoft.AspNetCore.Mvc;

namespace MovManagerr.Web.Controllers
{
    public class IptvController : Controller
    {
        private readonly MovieSearcher _movieSearcher;
        public IptvController()
        {
            _movieSearcher = new MovieSearcher();
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Content> contents = await _movieSearcher.GetAllMovies();

            return View(contents.Take(25));
        }
    }
}
