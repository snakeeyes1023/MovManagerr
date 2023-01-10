using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovManagerr.Explorer.Services;
using MovManagerr.Web.Infrastructure;

namespace MovManagerr.Controllers
{
    [ServiceFilter(typeof(AdminActionFilter))]
    public class HomeController : Controller
    {
        //constructor
        
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() {
            return View();
        }

        public IActionResult Iptv()
        {
            return RedirectToAction("Index", "Iptv");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View();
        }
    }
}
