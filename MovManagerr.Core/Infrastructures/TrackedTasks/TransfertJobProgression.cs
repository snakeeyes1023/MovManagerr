using MovManagerr.Core.Infrastructures.TrackedTasks.Generals;

namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class TransfertJobProgression : TrackedJobProgression
    {
        public TransfertJobProgression(string jobId, string origin, string destination) : base(jobId)
        {
            Origin = origin;
            Destination = destination;
        }     
      
        public string Origin { get; set; }
        public string Destination { get; set; }
    }
}




namespace MovManagerr.Core.Infrastructures.TrackedTasks.Generals
{
    public static partial class GlobalTrackedTask
    {

        /// <summary>
        /// Adds the transfert job if not exist.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static TransfertJobProgression AddTransfertJobIfNotExist(string jobId, string origin, string destination)
        {
            if (GetJobById(jobId) is TransfertJobProgression transfertJobProgression)
            {
                transfertJobProgression.Origin = origin;
                transfertJobProgression.Destination = destination;
                return transfertJobProgression;
            }
            else
            {
                transfertJobProgression = new TransfertJobProgression(jobId, origin, destination);
                return AddTrackedJob(transfertJobProgression);
            }
        }
    }
}