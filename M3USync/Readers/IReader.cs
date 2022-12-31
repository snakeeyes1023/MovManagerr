using M3USync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Readers
{
    public interface IReader : IDisposable
    {
        event Action OnContentProceeded;

        void Read(MediaM3u mediaInfo);

        void SyncInDatabase();
    }
}
