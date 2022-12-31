using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Models.Intefaces
{
    public interface ISave
    {
        public string Url { get; set; }
        string GetDirectoryPath();
        string GetFileName();
        string GetFullPath();
    }
}
