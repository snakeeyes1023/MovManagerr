using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Core.Helpers.Transferts
{
    public class TransfertContext
    {
        public string Destination { get; set; }
        public string Origin { get; set; }
        public bool Replace { get; set; }
        public bool TriggerPlexScan { get; set; }
        public bool DeleteOrigin { get; set; }      
        public ContentType ContentType { get; set; }


        internal bool ValidateMove()
        {
            if (string.IsNullOrWhiteSpace(Origin))
            {
                throw new ArgumentNullException(nameof(Origin), "The origin path cannot be null or empty.");
            }

            if (!File.Exists(Origin))
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulé : {Origin} vers {Destination} (Le fichier d'origine n'existe pas)", LogType.Error);
                throw new InvalidOperationException($"The file {Origin} does not exist.");
            }

            if (File.Exists(Destination) && !Replace)
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulé : {Origin} vers {Destination} (Le fichier existe déjà)", LogType.Error);
                throw new InvalidOperationException($"The file {Destination} already exists and Replace flag is false.");
            }

            return true;
        }
    }
}
