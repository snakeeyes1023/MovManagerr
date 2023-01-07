using M3USync.Downloaders.M3U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Downloaders.Contents.Readers
{
    public interface IReader : IDisposable
    {
        event Action OnContentProceeded;

        void Read(MediaM3u mediaInfo);

        void SyncInDatabase();
    }
}
