using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Extractors
{
    public class MovieExtractor : IMovieExtractor
    {
        public FileMovieNameInfo ExtractFromFileName(string fileName)
        {
            return new FileMovieNameInfo()
            {
                MovieName = Path.GetFileNameWithoutExtension(fileName),
                Extensions = Path.GetExtension(fileName)
            };
        }
    }

    public class FileMovieNameInfo
    {
        public string MovieName { get; set; }
        public int Year { get; set; }
        public string Extensions { get; set; }
    }
}
