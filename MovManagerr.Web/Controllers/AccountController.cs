using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovManagerr.Explorer.Services;
using MovManagerr.Models.ViewModels;

namespace MovManagerr.Controllers
{
    public class AccountController : Controller
    {
        //constructor
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel info)
        {
            if (info.Username == "admin" && info.Password == "Cote1!")
            {
                _httpContextAccessor.HttpContext.Session.SetString("username", info.Username);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Session.Remove("username");

            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
