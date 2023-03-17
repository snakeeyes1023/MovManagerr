namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class StatusChangedEventArgs : EventArgs
    {
        public TrackedJobStatus Status { get; }

        public StatusChangedEventArgs(TrackedJobStatus status)
        {
            Status = status;
        }
    }
}
