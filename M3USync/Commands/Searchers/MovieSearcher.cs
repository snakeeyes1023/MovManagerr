using Hangfire;
using M3USync.Data;
using M3USync.Http;
using M3USync.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M3USync.Program;

namespace M3USync.Commands
{
    public class MovieSearcher : Searcher<Movie>
    {
        public MovieSearcher() : base("film")
        {
        }
    }
}
