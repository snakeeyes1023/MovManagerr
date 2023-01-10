using M3USync.Commands.Searchers;
using M3USync.Data.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace MovManagerr.Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieSearcherCommand _movieSearcher;
        public MovieController()
        {
            _movieSearcher = new MovieSearcherCommand();
        }
        
        public async Task<IActionResult> RecentlyAdded()
        {
            IEnumerable<Content> contents = await _movieSearcher.GetAllContentsAsync();

            return View(contents.Take(25));
        }
    }
}
