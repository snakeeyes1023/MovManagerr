using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public static class GlobalTrackedTask
    {
        public static IEnumerable<TrackedJobProgression> TrackedJobs
        {
            get
            {
                return _trackedJobs;
            }
        }

        public static event Action OnJobChanged;

        private static List<TrackedJobProgression> _trackedJobs = new List<TrackedJobProgression>();

        public static T AddTrackedJob<T>(T trackedJob) where T : TrackedJobProgression
        {
            if (_trackedJobs.FirstOrDefault(x => x.JobId == trackedJob.JobId) is T job)
            {
                return job;
            }
            else
            {
                _trackedJobs.Add(trackedJob);
                OnJobChanged?.Invoke();
                return trackedJob;
            }
        }   
        
        public static void RemoveJob<T>(T trackedJob) where T : TrackedJobProgression
        {
            _trackedJobs = _trackedJobs.Where(x => x != trackedJob).ToList();
            OnJobChanged?.Invoke();
        }

        public static TrackedJobProgression? GetJobById(string id)
        {
            return _trackedJobs.FirstOrDefault(x => x.JobId == id);
        }
    }
}
