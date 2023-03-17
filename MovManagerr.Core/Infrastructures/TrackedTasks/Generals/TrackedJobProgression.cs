namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class TrackedJobProgression
    {
        private int _progress;
        private TrackedJobStatus _status;

        internal TrackedJobProgression(string jobId)
        {
            JobId = jobId;
            CreationDate = DateTime.Now;
            ElapsedTime = TimeSpan.Zero;
            Timer = new System.Timers.Timer(1000); // Timer avec un intervalle d'1 seconde (1000 ms)
            Timer.Elapsed += (sender, args) => ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
        }

        public TrackedJobProgression() : this(string.Empty)
        {
            Progress = 0;
            Status = TrackedJobStatus.Pending;
        }

        public string JobId { get; private set; }

        public DateTime CreationDate { get; private set; }
        
        public TimeSpan ElapsedTime { get; private set; }

        public System.Timers.Timer Timer { get; private set; }


        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    ProgressChanged?.Invoke(this);

                    if (_progress == 100)
                    {
                        Status = TrackedJobStatus.Succeeded;
                    }
                    else if (_progress > 0)
                    {
                        Status = TrackedJobStatus.Processing;
                    }

                }
            }
        }

        public TrackedJobStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs(_status));


                    if (_status == TrackedJobStatus.Processing)
                    {
                        Timer.Start();
                    }
                    else
                    {
                        Timer.Stop();
                    }
                }
            }
        }

        public event Action<TrackedJobProgression> ProgressChanged;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;


    }
}
