using Hangfire;
using Hangfire.Server;
using MovManagerr.Core.Helpers.Transferts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Infrastructures.TrackedTasks;
using Xabe.FFmpeg;

namespace MovManagerr.Core.Helpers.Transcode
{
    public class TranscodeHelper
    {
        public string _destinationPath;
        public string _actualPath;
        public bool _replaceDestination;

        public bool _useCustomTranscodeFolder => !string.IsNullOrWhiteSpace(_transcodeFolder);
        public readonly string _transcodeFolder;
        public readonly string _doneTranscodeFolder;

        public TranscodeHelper() 
        {
            _transcodeFolder = Preferences.Instance.Settings.TranscodeConfiguration.DirectoryPath;
            Directory.CreateDirectory(_transcodeFolder);

            if (_useCustomTranscodeFolder)
            {
                _doneTranscodeFolder = Path.Combine(_transcodeFolder, "Done");
                Directory.CreateDirectory(_doneTranscodeFolder);
            }
        }

        public static TranscodeHelper New()
        {
            return new TranscodeHelper();
        }

        public TranscodeHelper From(string originPath)
        {
            _actualPath = originPath;
            return this;
        }

        public TranscodeHelper To(string finalPath)
        {
            _destinationPath = finalPath;
            return this;
        }

        public TranscodeHelper ReplaceDestination(bool replace)
        {
            _replaceDestination = replace;
            return this;
        }

        public void EnqueueRun()
        {
            SimpleLogger.AddLog("Ajout d'un fichier en attente de transcodage...", LogType.Info);

            string jobId = BackgroundJob.Enqueue(() => Run(this, CancellationToken.None, null));
            GlobalTrackedTask.AddTrackedJob(new TranscodeJobProgression(jobId, _actualPath));      
        }

        [Queue("transcode")]
        public async Task Run(TranscodeHelper helper,  CancellationToken cancellationToken, PerformContext? context)
        {
            _replaceDestination = helper._replaceDestination;
            _destinationPath = helper._destinationPath;
            _actualPath = helper._actualPath;

            TranscodeJobProgression progression;

            if (context != null && GlobalTrackedTask.GetJobById(context.BackgroundJob.Id) is TranscodeJobProgression transcode)
            {
                progression = transcode;
            }
            else
            {
                progression = GlobalTrackedTask.AddTrackedJob(new TranscodeJobProgression(context?.BackgroundJob.Id ?? string.Empty, _actualPath));
            }

            if (_useCustomTranscodeFolder)
            {
                MoveToTranscodeFolder();
            }

            string transcodeDestination = GetTranscodeFilePath(_actualPath);

            SimpleLogger.AddLog($"Transcodage du film en cours vers {transcodeDestination} ...");
            
            IConversionResult conversionResult = await Transcode(_actualPath, transcodeDestination, cancellationToken, progression);
            SimpleLogger.AddLog($"Transcodage terminé après {conversionResult.Duration.ToString("h'h 'm'm 's's'")}");

            MoveTo(_destinationPath, _replaceDestination);
        }

        
        private async Task<IConversionResult> Transcode(string origin, string transcodeDestination, CancellationToken cancellationToken, TranscodeJobProgression progression)
        {
            string ffmpegString = Preferences.Instance.Settings.TranscodeConfiguration.GetTranscodeFFmpegString(origin, transcodeDestination);

            IConversion conversion = FFmpeg.Conversions.New();

            conversion.OnProgress += (sender, args) =>
            {
                progression.Progress = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);     
            };

            IConversionResult conversionResult = await conversion.Start(ffmpegString, cancellationToken);

            if (conversionResult != null)
            {
                _actualPath = transcodeDestination;

                SimpleLogger.AddLog(new NotificationLog("Transcodage terminé", "Le film a été transcodé avec succès !"), LogType.Info);

                return conversionResult;
            }
            else
            {
                throw new Exception("Le transcodage à échoué");
            }
        }

        private void MoveToTranscodeFolder()
        {
            string beforeTranscodePath = Path.Combine(_transcodeFolder, Path.GetFileName(_actualPath));

            if (File.Exists(beforeTranscodePath))
            {
                beforeTranscodePath = GetUnifyFilePath(beforeTranscodePath);
            }

            if (_actualPath != beforeTranscodePath)
            {
                SimpleLogger.AddLog("Copie du fichier dans le dossier de transcodage...");
                File.Copy(_actualPath, beforeTranscodePath);
                _actualPath = beforeTranscodePath;
                SimpleLogger.AddLog("Copie du fichier terminé...");
            }
        }

        private void MoveTo(string destination, bool replace = false)
        {
            if (_actualPath != destination)
            {
                var transfert = TransfertHelper.New()
                    .From(_actualPath)
                    .To(destination)
                    .Replace(replace);

                transfert.MoveFile(false);
                
                _actualPath = destination;
            }
        }

        private string GetTranscodeFilePath(string path)
        {
            string transcodePath;

            if (_useCustomTranscodeFolder)
            {
                transcodePath = Path.Combine(_doneTranscodeFolder, Path.GetFileNameWithoutExtension(path) + " [Transcode]" + Path.GetExtension(path));
            }
            else
            {
                transcodePath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileNameWithoutExtension(path) + " [Transcode]" + Path.GetExtension(path));
            }

            while (File.Exists(transcodePath))
            {
                transcodePath = GetUnifyFilePath(transcodePath);
            }

            return transcodePath;
        }

        private static string GetUnifyFilePath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileNameWithoutExtension(path) + "_1" + Path.GetExtension(path));
        }
    }
}
