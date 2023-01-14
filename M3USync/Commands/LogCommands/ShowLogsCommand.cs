using M3USync.Downloaders.Contents;
using M3USync.Infrastructures.UIs;

namespace M3USync.Commands.LogCommands
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
