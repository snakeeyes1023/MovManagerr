using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Configurations
{
    public class PlexConfiguration
    {
        public virtual string ApiKey { get; set; }

        public virtual IList<PathEquivalent> PathEquivalent { get; private set; }


        public PlexConfiguration()
        {
            PathEquivalent = new List<PathEquivalent>();
        }

        public string GetEquivalentPath(string path)
        {
            path = path.Replace("/", @"\");
            foreach (var equivalent in PathEquivalent)
            {
                equivalent.OnDisk = equivalent.OnDisk.Replace("/", @"\");
                equivalent.OnPlex = equivalent.OnPlex.Replace("/", @"\");
            }

            if (PathEquivalent.FirstOrDefault(x => path.Contains(x.OnPlex, StringComparison.CurrentCultureIgnoreCase)) is PathEquivalent pathEquivalent)
            {
                return path.Replace(pathEquivalent.OnPlex, pathEquivalent.OnDisk);
            }

            return path;
        }
    }

    public class PathEquivalent
    {
        public virtual string OnPlex { get; set; } = string.Empty;
        public virtual string OnDisk { get; set; } = string.Empty;
    }
}
