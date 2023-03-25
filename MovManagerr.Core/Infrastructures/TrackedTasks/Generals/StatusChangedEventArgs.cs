namespace MovManagerr.Core.Infrastructures.TrackedTasks.Generals
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
