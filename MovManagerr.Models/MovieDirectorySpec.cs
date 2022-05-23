using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Models
{
    public class MovieDirectorySpec
    {
        public int Id { get; set; }
        public MovieDirectory DirectoryInfo { get; set; }
        public MovieFile File { get; set; }
        public int NbFiles { get; set; }

        public bool IsNeedAttention { get { return NeedAttention(); } }
        public bool NeedAttention() => NbFiles > 1 || File.NeedCompression() || !File.IsValidProvider();

    }
}
