using MovManagerr.Cls.Commands;
using MovManagerr.Core.Downloaders.Contents;
using MovManagerr.Core.Infrastructures.Loggers;

namespace MovManagerr.Cls.Commands.LogCommands
{
    public class ShowLogsCommand : Command
    {
        public ShowLogsCommand() : base("Afficher les logs de Téléchargement", false, false)
        {
        }

        protected override void Start()
        {
            SimpleLogger.AddLog("Appuyer sur ctrl-x pour quitter les logs");

            var downloaderInstance = ContentDownloaderClient.Instance;

            //SimpleLogger. += (log) => log.Log();

            //foreach (var log in downloaderInstance.Logs)
            //{
            //    log.Log();
            //}

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);
            } while (cki.Key != ConsoleKey.X || cki.Modifiers != ConsoleModifiers.Control);
        }
    }
}
