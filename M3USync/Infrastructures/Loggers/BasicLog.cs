using M3USync.Infrastructures.UIs;

namespace M3USync.Infrastructures.Loggers
{
    public class BasicLog : ILog
    {
        public string Message { get; set; }
        public void Log()
        {
            AwesomeConsole.WriteInfo(Message);
        }
    }
}
