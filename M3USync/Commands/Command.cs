using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands
{
    public abstract class Command
    {
        public readonly string CommandName;
        public readonly string EndMessage;

        public Command(string commandName, string endMessage)
        {
            CommandName = commandName;
            EndMessage = endMessage;
        }

        public void Execute()
        {
            Console.WriteLine("Début de le la commande : " + CommandName);

            try
            {
                Start();

                Console.WriteLine(EndMessage);

            }
            catch (Exception e)
            {
                Console.WriteLine("Une erreur est survenue : " + e.Message);
            }
        }

        protected abstract void Start();        
    }
}
