using M3USync.UIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Http.Models
{
    public class BasicLog : ILog
    {
        public string Message { get; set; }
        public void Log()
        {
            AwesomeConsole.WriteInfo(Message);
        }
    }
}
