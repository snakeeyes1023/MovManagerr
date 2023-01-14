using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Infrastructures.Loggers
{
    public interface ILog
    {
        public string Message { get; }
    }

    public interface ILog<T> : ILog
    {
    }
}
