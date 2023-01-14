namespace MovManagerr.Core.Infrastructures.Loggers
{
    public interface ILog
    {
        public string Message { get; }
    }

    public interface ILog<T> : ILog
    {
    }
}
