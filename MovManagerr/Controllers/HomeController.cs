using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovManagerr.Explorer.Services;

namespace MovManagerr.Controllers
{
    public class HomeController : Controller
    {
        //constructor
        private readonly ContentServices _contentServices;
        
        public HomeController(ContentServices contentServices)
        {
            _contentServices = contentServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View();
        }
    }
}
