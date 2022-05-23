using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using MovManagerr.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using MovManagerr.Explorer.Services;

namespace MovManagerr.Controllers {

    [Route("api/[controller]")]
    public class MovieDataController : Controller
    {
        private readonly ContentServices _contentServices;

        public MovieDataController(ContentServices contentServices)
        {
            _contentServices = contentServices;

        }

        [HttpGet]
        public object Get(DataSourceLoadOptions loadOptions) {

            var movies = _contentServices.GetAllMoviesFromFiles();

            return DataSourceLoader.Load(movies, loadOptions);
        }

    }
}