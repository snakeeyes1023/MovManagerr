using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Models
{
    public class MovieFile
    {
        
        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        public double Gb { get; set; }
        public DateTime AddedDate { get; set; }

        public bool NeedCompression() => Gb > 5;
        
        public bool IsValidProvider()
        {
            return Name.Split("[ Torrent911.net ]").Length == 1;
        }

        public bool IsWebRip()
        {
            return Name.Split("WEBRip").Length > 1;
        }
    }
}
