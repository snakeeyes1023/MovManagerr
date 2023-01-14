using M3USync.Infrastructures.UIs;

namespace M3USync.Infrastructures.Loggers
{
    public class BasicLog<T> : ILog<T> 
    {
        public string Message { get; set; }
        public int Level { get; set; } = 0;

        public BasicLog(string message)
        {
            Message = message;
        }

    }
}
