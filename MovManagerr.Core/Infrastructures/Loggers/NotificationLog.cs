namespace MovManagerr.Core.Infrastructures.Loggers
{
    public class NotificationLog : ILog
    {
        public NotificationLog(string title, string message)
        {
            Message = message;
            Title = title;
        }

        public string Title { get; private set; }
        public string Message { get; private set; }
    }
}
