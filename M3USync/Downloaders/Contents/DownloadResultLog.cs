using M3USync.Infrastructures.Loggers;
using M3USync.Infrastructures.UIs;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Downloaders.Contents
{
    public class DownloadResultLog : ILog
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public bool HasSucceeded { get; set; }
        public int Attempts { get; set; }

        public Exception? Exception { get; set; }
        public DateTime StartedTimeJob { get; set; }
        public DateTime EndedTimeJob { get; set; }

        public void Log()
        {
            if (HasSucceeded)
            {
                AwesomeConsole.WriteSuccess($"Téléchargement de {Origin} vers {Destination} réussi");
            }
            else
            {
                AwesomeConsole.WriteError($"Téléchargement de {Origin} vers {Destination} échoué après {Attempts} tentatives");
            }
        }
    }
}
