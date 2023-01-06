using M3USync.Config;
using M3USync.Http;
using M3USync.Http.Models;
using M3USync.UIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands
{
    public class ShowLogsCommand : Command
    {
        public ShowLogsCommand() : base("Afficher les logs de Téléchargement", false, false)
        {
        }

        protected override void Start()
        {
            AwesomeConsole.WriteWarning("Appuyer sur ctrl-x pour quitter les logs");

            var downloaderInstance = ContentDownloader.Instance;

            downloaderInstance.OnNewLogAdded += (ILog log) => log.Log();

            foreach (var log in downloaderInstance.Logs)
            {
                log.Log();
            }

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);
            } while (cki.Key != ConsoleKey.X || cki.Modifiers != ConsoleModifiers.Control);
        }
    }
}
