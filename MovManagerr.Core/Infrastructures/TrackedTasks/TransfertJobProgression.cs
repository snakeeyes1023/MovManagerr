using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class TransfertJobProgression : TrackedJobProgression
    {
        public TransfertJobProgression(string jobId, string origin, string destination) : base(jobId)
        {
            Origin = origin;
            Destination = destination;
        }     
      
        public string Origin { get; set; }
        public string Destination { get; set; }
    }
}
