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
using Hangfire;
using Newtonsoft.Json;

namespace MovManagerr.Controllers
{

    [Route("api/[controller]")]
    public class MovieDataController : Controller
    {
        private readonly ContentServices _contentServices;
        private readonly IBackgroundJobClient _backgroundJobs;

        public MovieDataController(ContentServices contentServices, IBackgroundJobClient backgroundJobs)
        {
            _contentServices = contentServices;
            _backgroundJobs = backgroundJobs;
        }

        [HttpGet]
        public async Task<object> Get(DataSourceLoadOptions loadOptions)
        {

            var movies = await _contentServices.GetAllMoviesFromFilesAsync();

            return DataSourceLoader.Load(movies, loadOptions);
        }

        [HttpPost]
        public object Batch([FromBody] List<DataChange> changes)
        {
            foreach (var change in changes)
            {
                if ( change.Type == "remove")
                {
                    _backgroundJobs.Enqueue<ContentServices>(x => x.DeleteMovie(Convert.ToInt32(change.Key.ToString())));
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