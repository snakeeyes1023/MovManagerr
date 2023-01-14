namespace MovManagerr.Core.Infrastructures.Loggers
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