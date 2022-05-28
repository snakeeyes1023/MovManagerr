using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Explorer.Config
{
    public class FtpConfig
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }

    }
}
