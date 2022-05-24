using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Tmdb.Config
{
    public class TmdbConfig
    {
        public string ApiKey { get; set; }
        public string Session { get; set; }
        public string Url { get; set; }
        public string Language { get; set; }
        public bool UseSsl { get; set; }
    }
}
