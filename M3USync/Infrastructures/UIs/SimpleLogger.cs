using M3USync.Infrastructures.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Infrastructures.UIs
{
    public class TypedLog 
    {
        public TypedLog(ILog log, LogType type = LogType.Unknown)
        {
            Log = log;
            Type = type;
            CreatedDate = DateTime.Now;
        }
        public LogType Type { get; set; }
        public ILog Log { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CssClass
        {
            get
            {
                switch (Type)
                {
                    case LogType.Unknown:
                        return "default";
                    case LogType.Info:
                        return "info";
                    case LogType.Warning:
                        return "warning";
                    case LogType.Error:
                        return "error";
                    default:
                        return "default";
                }
            }
        }
    }

    public enum LogType
    {
        Unknown = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public static class SimpleLogger
    {
        public static event Action<TypedLog> OnLogged;

        public static List<TypedLog> Logs = new List<TypedLog>();

        class simpleLogGenerator { };

        public static void AddLog(string message, LogType type = LogType.Unknown)
        {
            var basicLog = new BasicLog<simpleLogGenerator>(message);
            AddLog(basicLog, type);
        }

        public static void AddLog(ILog log, LogType type = LogType.Unknown)
        {
            TypedLog createdLog = new TypedLog(log, type);
            Logs.Add(createdLog);
            OnLogged?.Invoke(createdLog);
        }
    }
}