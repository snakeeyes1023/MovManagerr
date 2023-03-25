using MovManagerr.Core.Helpers.Transferts;
using MovManagerr.Core.Infrastructures.TrackedTasks;
using MovManagerr.Core.Infrastructures.TrackedTasks.Generals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{

    public class PlexImportJobProgression : TrackedJobProgression
    {
        public PlexImportJobProgression(string jobId) : base(jobId)
        {
        }
    }


}