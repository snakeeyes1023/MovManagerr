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
using Newtonsoft.Json;
using MovManagerr.Web.Infrastructure;

namespace MovManagerr.Controllers
{

    [ServiceFilter(typeof(AdminActionFilter))]
    [Route("api/[controller]")]
    public class MovieDataController : Controller
    {
        private readonly ContentServices _contentServices;
        private readonly MovieServices _movieService;
        //private readonly IBackgroundJobClient _backgroundJobs;

        public MovieDataController(
            ContentServices contentServices,
            //IBackgroundJobClient backgroundJobs,
            MovieServices movieServices)
        {
            _contentServices = contentServices;
            //_backgroundJobs = backgroundJobs;
            _movieService = movieServices;
        }

        [HttpGet]
        public async Task<object> Get(DataSourceLoadOptions loadOptions)
        {
            List<MovieDirectorySpec> movies = new List<MovieDirectorySpec>();

            await foreach (var movie in _contentServices.GetAllMoviesFromFilesAsync(loadOptions.Take, loadOptions.Skip))
            {
                movies.Add(movie);
            }
            
            return DataSourceLoader.Load(movies, loadOptions);
        }

        [HttpPost]
        public object Batch([FromBody] List<DataChange> changes)
        {
            foreach (var change in changes)
            {
                if (change.Type == "remove")
                {
                   // _backgroundJobs.Enqueue<MovieServices>(x => x.FullDeleteMovie(Convert.ToInt32(change.Key.ToString())));
                }          
            }


            return Ok(changes);
        }
        public class DataChange
        {
            [JsonProperty("key")]
            public object Key { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("data")]
            public object Data { get; set; }
        }
    }


}