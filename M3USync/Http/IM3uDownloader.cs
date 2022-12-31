using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M3USync.Http.Models;
using M3USync.Models;

namespace M3USync.Http
{
    public interface IM3uDownloader
    {
        Task<bool> Start(string destinationPath = "");
    }
}
