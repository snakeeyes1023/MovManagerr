using M3USync.Http;
using M3USync.Http.Models;
using M3USync.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.UIs
{
    public class DownloaderGlobalUI
    {
        private int _totalMovies;
        
        public int TotalMovies
        {
            get => _totalMovies;
            set
            {
                UpdateUI();
                _totalMovies = value;
            }
        }


        private int _totalSeries;

        public int TotalSeries
        {
            get => _totalSeries;
            set
            {
                UpdateUI();
                _totalSeries = value;
            }
        }


        public DownloaderGlobalUI(IEnumerable<IReader> readers)
        {

            foreach (var reader in readers)
            {
                if (reader is MovieReader)
                {
                    reader.OnContentProceeded += () => { TotalMovies++; };
                }
                else if (reader is SerieReader)
                {
                    reader.OnContentProceeded += () => { TotalSeries++; };
                }
            }        
        }

        public void UpdateUI()
        {
            // On efface la ligne actuelle de la console
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            // On affiche le nombre de films et de séries traités

            AwesomeConsole.WriteInfo($"Films traités : {TotalMovies}, Séries traitées : {TotalSeries}");
        }
    }
}
