using Hangfire;
using Hangfire.Server;
using MovManagerr.Core.Helpers.PlexScan;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Infrastructures.TrackedTasks;

namespace MovManagerr.Core.Helpers.Transferts
{
    public class TransfertHelper
    {
        public string _Destination { get; set; }
        public string _Origin { get; set; }
        public bool _Replace { get; set; }
        public bool _TriggerPlexScan { get; set; }

        public static TransfertHelper New()
        {
            return new TransfertHelper()
            {
                _TriggerPlexScan = true
            };
        }

        public TransfertHelper To(string destinationPath)
        {
            _Destination = destinationPath;
            return this;
        }
        public TransfertHelper From(string origin)
        {
            _Origin = origin;
            return this;
        }

        public TransfertHelper Replace(bool replace)
        {
            _Replace = replace;
            return this;
        }

        public TransfertHelper TriggerPlexScan(bool enable)
        {
            _TriggerPlexScan = enable;
            return this;
        }

        public string MoveFile(bool enqueue = true)
        {
            if (enqueue)
            {
                string jobId = BackgroundJob.Enqueue(() => MoveFileWithProgress(this, CancellationToken.None, null));
                CreateProgressJob(jobId);
                return jobId;
            }
            else
            {
                MoveFileWithProgress(this, CancellationToken.None, null);
                return string.Empty;
            }
        }

        [Queue("file-transfert")]
        public void MoveFileWithProgress(TransfertHelper? helper, CancellationToken cancellationToken, PerformContext? context)
        {
            if (helper != null)
            {
                _Destination = helper._Destination;
                _Origin = helper._Origin;
                _Replace = helper._Replace;
                _TriggerPlexScan = helper._TriggerPlexScan;
            }

            SimpleLogger.AddLog($"Déplacement du fichier {_Origin} vers {_Destination} en cours...");


            TransfertJobProgression progression;

            if (context != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is TransfertJobProgression transfert)
            {
                progression = transfert;
            }
            else
            {
                progression = CreateProgressJob(context?.BackgroundJob.Id ?? string.Empty);
            }

            if (ValidateMove())
            {
                MoveFileWithProgress(progression, cancellationToken);

                TriggerScanIfRequired();

                SimpleLogger.AddLog($"Déplacement du fichier {_Origin} vers {_Destination} terminée", LogType.Info);
            }
        }

        private TransfertJobProgression CreateProgressJob(string jobId)
        {
            return GlobalTrackedTask.AddTrackedJob(new TransfertJobProgression(jobId, _Origin, _Destination));
        }

        private void MoveFileWithProgress(TrackedJobProgression trackedJobProgression, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 1024; // 1 Mo
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            long fileLength = new FileInfo(_Origin).Length;
            long totalBytesRead = 0;

            using (FileStream sourceStream = new FileStream(_Origin, FileMode.Open, FileAccess.Read))
            {
                using (FileStream destinationStream = new FileStream(_Destination, _Replace ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
                {
                    while ((bytesRead = sourceStream.Read(buffer, 0, bufferSize)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        destinationStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        int progress = (int)((float)totalBytesRead / fileLength * 100);
                        trackedJobProgression.Progress = progress;
                    }
                }
            }

            if (!_Replace)
            {
                File.Delete(_Origin);
            }
        }

        private bool ValidateMove()
        {
            if (File.Exists(_Destination) && !_Replace)
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulée : {_Origin} vers {_Destination} (Le fichier existe déjà)", LogType.Error);
                throw new InvalidOperationException($"Le fichier {_Destination} existe déjà");
            }
            if (!File.Exists(_Origin))
            {
                SimpleLogger.AddLog($"Déplacement du fichier annulée : {_Origin} vers {_Destination} (Le fichier d'origine n'existe pas)", LogType.Error);
                throw new InvalidOperationException("Le fichier d'origine n'existe pas");
            }
            
            return true;
        }

        private void TriggerScanIfRequired()
        {
            var plexConfiguration = Preferences.Instance.Settings.PlexConfiguration;

            if (_TriggerPlexScan
                && plexConfiguration.IsConfigured
                && plexConfiguration.TriggerScanOnMoved)
            {
                BackgroundJob.Enqueue<PlexScanHelper>(x => x.Scan());
            }
        }
    }
}
