﻿using M3USync.Infrastructures.Loggers;
using M3USync.Infrastructures.UIs;

namespace M3USync.Downloaders.Contents
{
    public class DownloadResultLog<T> : ILog<T>
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public bool HasSucceeded { get; set; }
        public int Attempts { get; set; }

        public Exception? Exception { get; set; }
        public DateTime StartedTimeJob { get; set; }
        public DateTime EndedTimeJob { get; set; }

        public string Message => $"Téléchargement de {Origin} vers {Destination} : {(HasSucceeded ? "réussi" : "échoué")} après {Attempts} tentatives.";
    }
}
