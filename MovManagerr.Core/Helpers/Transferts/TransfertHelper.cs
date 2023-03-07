using Hangfire;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Transferts
{
    public class TransfertHelper
    {
        public string _Destination { get; set; }
        public string _Origin { get; set; }
        public bool _Replace { get; set; }

        public static TransfertHelper New()
        {
            return new TransfertHelper();
        }

        public TransfertHelper To(string destinationPath)
        {
            _Destination = destinationPath;
            return this;
        }
        public TransfertHelper From(string origin)
        {
            _Origin = origin;
            return this;
        }

        public TransfertHelper Replace(bool replace)
        {
            _Replace = replace;
            return this;
        }

        public void EnqueueRun()
        {
            BackgroundJob.Enqueue(() => Run(this));
        }

        public void Run()
        {
            Run(this);
        }


        [Queue("file-transfert")]
        public void Run(TransfertHelper? helper)
        {
            if (helper != null)
            {
                _Destination = helper._Destination;
                _Origin = helper._Origin;
                _Replace = helper._Replace;
            }

            SimpleLogger.AddLog($"Déplacement du fichier {_Origin} vers {_Destination} en cours...");

            if (File.Exists(_Destination) && !_Replace)
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulée : {_Origin} vers {_Destination} (Le fichier existe déjà)", LogType.Error);
                throw new InvalidOperationException($"Le fichier {_Destination} existe déjà");
            }
            if (!File.Exists(_Origin))
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulée : {_Origin} vers {_Destination} (Le fichier d'origine n'existe pas)", LogType.Error);
                throw new InvalidOperationException("Le fichier d'origine n'existe pas");
            }

            File.Move(_Origin, _Destination, _Replace);

            SimpleLogger.AddLog($"Déplacement du fichier {_Origin} vers {_Destination} terminée", LogType.Info);
        }
    }
}
