using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class HangfireTaskProgressUpdaterAttribute : JobFilterAttribute,
    IServerFilter, IElectStateFilter
    {
        public void OnPerforming(PerformingContext context)
        {
            if (context.BackgroundJob != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is TrackedJobProgression progression)
            {
                progression.Status = TrackedJobStatus.Processing;
            }
        }

        public void OnPerformed(PerformedContext context)
        {
            if (context.BackgroundJob != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is TrackedJobProgression progression)
            {

                if (context.Exception != null)
                {
                    progression.Status = TrackedJobStatus.Failed;
                }
                else
                {
                    progression.Status = TrackedJobStatus.Succeeded;
                }
            }
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                if (context.BackgroundJob != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is TrackedJobProgression progression)
                {
                    progression.Status = TrackedJobStatus.Failed;
                }
            }
        }
    }
}
