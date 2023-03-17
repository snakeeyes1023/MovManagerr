using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class HangfireTaskProgressUpdaterAttribute : JobFilterAttribute,
    IClientFilter, IServerFilter, IElectStateFilter
    {
        public void OnCreating(CreatingContext context)
        {
            //Logger.InfoFormat("Creating a job based on method `{0}`...", context.Job.Method.Name);
        }

        public void OnCreated(CreatedContext context)
        {
            //Logger.InfoFormat(
            //    "Job that is based on method `{0}` has been created with id `{1}`",
            //    context.Job.Method.Name,
            //    context.BackgroundJob?.Id);
        }

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
                progression.Status = TrackedJobStatus.Succeeded;
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
