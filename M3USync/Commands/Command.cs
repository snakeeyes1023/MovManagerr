using M3USync.UIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands
{
    public abstract class Command
    {
        public readonly string CommandName;
        public readonly bool timed;
        private readonly bool WaitUserInteraction;

        public event Action OnStarted;
        public event Action OnEnded;

        private Stopwatch TimeProcessing;

        public Command(string commandName, bool timed = true, bool waitUserInteraction = true)
        {
            CommandName = commandName;
            WaitUserInteraction = waitUserInteraction;
            if (timed)
            {
                OnStarted += StartTimer;
                OnEnded += StopTimer;
            }
        }

        public void StartTimer()
        {
            AwesomeConsole.WriteSuccess("Début de le la commande : " + CommandName);
            TimeProcessing = Stopwatch.StartNew();
        }

        public void StopTimer()
        {
            TimeProcessing.Stop();
            AwesomeConsole.WriteInfo("Temps de traitement de la commande : " + TimeProcessing.ElapsedMilliseconds / 1000 + "s et " + TimeProcessing.ElapsedMilliseconds + "ms");
        }


        public void Execute()
        {  
            try
            {
                OnStarted?.Invoke();
                Start();
            }
            catch (Exception e)
            {
                AwesomeConsole.WriteError("Une erreur est survenue : " + e.Message);
            }
            finally
            {
                OnEnded?.Invoke();

                if (WaitUserInteraction)
                {
                    Console.WriteLine("Appuyer sur une touche pour continuer ...");
                    Console.ReadKey();
                }
            }
        }

        

        protected abstract void Start();        
    }
}
