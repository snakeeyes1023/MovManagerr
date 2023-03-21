namespace MovManagerr.Core.Infrastructures.TrackedTasks
{
    public class TranscodeJobProgression : TrackedJobProgression
    {
        public TranscodeJobProgression(string jobId, string fileInTranscode) : base(jobId)
        {
            FileInTranscoding = fileInTranscode;
        }     
      
        public string FileInTranscoding { get; set; }
    }
}
