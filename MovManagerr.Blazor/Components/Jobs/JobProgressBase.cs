using Microsoft.AspNetCore.Components;
using MovManagerr.Core.Infrastructures.TrackedTasks;
using System;
using System.Timers;

namespace MovManagerr.Blazor.Components.Jobs
{
    public class JobProgressBase<T> : ComponentBase, IDisposable where T : TrackedJobProgression
    {
        [Parameter]
        public T Job { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            Job.ProgressChanged += OnProgressChanged;
            Job.StatusChanged += OnStatusChanged;
            Job.Timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            InvokeAsync(() => StateHasChanged());
        }

        protected virtual void OnProgressChanged(TrackedJobProgression progression)
        {
            InvokeAsync(() => StateHasChanged());
        }

        protected virtual void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            InvokeAsync(() => StateHasChanged());
        }

        public virtual void Dispose()
        {
            Job.ProgressChanged -= OnProgressChanged;
            Job.StatusChanged -= OnStatusChanged;
            Job.Timer.Elapsed -= OnTimerElapsed;
        }

        public virtual void Delete()
        {
            if (Job.Status != TrackedJobStatus.Processing)
            {
                GlobalTrackedTask.RemoveJob(Job);
            }   
        }
    }
}
