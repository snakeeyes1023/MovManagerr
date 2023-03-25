using Hangfire;
using Hangfire.Server;
using MovManagerr.Core.Helpers.PlexScan;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Infrastructures.TrackedTasks;
using MovManagerr.Core.Infrastructures.TrackedTasks.Generals;

namespace MovManagerr.Core.Helpers.Transferts
{
    public abstract class BaseTransfertHelper : IDisposable
    {
        protected event EventHandler<TransfertContext> OnTransfertStarted;
        protected event EventHandler<TransfertContext> OnTransfertEnded;

        public BaseTransfertHelper()
        {
            OnTransfertStarted += (object? sender, TransfertContext args) =>
            {
                SimpleLogger.AddLog($"Déplacement du fichier {args.Origin} vers {args.Destination} en cours...");
            };
            OnTransfertEnded += (object? sender, TransfertContext args) =>
            {
                SimpleLogger.AddLog($"Déplacement du fichier {args.Origin} vers {args.Destination} terminée", LogType.Info);
            };
        }


        [Queue("file-transfert")]
        public void MoveFileWithProgress(TransfertContext transfertContext, CancellationToken cancellationToken, PerformContext? context)
        {
            OnTransfertStarted?.Invoke(this, transfertContext);

            TransfertJobProgression progression = GlobalTrackedTask.AddTransfertJobIfNotExist(context.BackgroundJob.Id, transfertContext.Origin, transfertContext.Destination);

            // Valide le transfert et lance une exception si le transfert est invalide
            if (transfertContext.ValidateMove())
            {
                StartMoveFile(transfertContext, progression, cancellationToken);

                OnTransfertEnded?.Invoke(this, transfertContext);

                if (transfertContext.DeleteOrigin)
                {
                    DeleteOrigin(transfertContext);
                }
                if (transfertContext.TriggerPlexScan)
                {
                    BackgroundJob.Enqueue<PlexScanHelper>(x => x.Scan());
                }
            }
        }

        /// <summary>
        /// Move the file from the origin to the destination byte per byte.
        /// </summary>
        /// <param name="transfertContext"></param>
        /// <param name="trackedJobProgression"></param>
        /// <param name="cancellationToken"></param>
        protected virtual void StartMoveFile(TransfertContext transfertContext, TrackedJobProgression trackedJobProgression, CancellationToken cancellationToken)
        {
            const int bufferSize = 1024 * 1024; // 1 Mo
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            long fileLength = new FileInfo(transfertContext.Origin).Length;
            long totalBytesRead = 0;

            using (FileStream sourceStream = new FileStream(transfertContext.Origin, FileMode.Open, FileAccess.Read))
            {
                FileMode mode = transfertContext.Replace ? FileMode.Create : FileMode.CreateNew;

                using (FileStream destinationStream = new FileStream(transfertContext.Destination, mode, FileAccess.Write))
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
        }

        /// <summary>
        /// Delete the origin file if it exists.
        /// </summary>
        /// <param name="transfertContext"></param>
        protected virtual void DeleteOrigin(TransfertContext transfertContext)
        {
            if (File.Exists(transfertContext.Origin))
            {
                File.Delete(transfertContext.Origin);
            }
        }

        public virtual void Dispose()
        {
            // left empty
        }
    }
}
