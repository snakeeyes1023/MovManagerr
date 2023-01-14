using M3USync.Infrastructures.UIs;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace M3USync.Commands.Backgrounds
{

    public abstract class EventedBackgroundService : IHostedService, IDisposable
    {
        #region Props

        public delegate void BackgroundEvent(EventedBackgroundService instance, EventArgs e);

        #region Events

        public event BackgroundEvent OnStarted;
        
        public event BackgroundEvent OnStoped;
        
        public event Action OnStatusChanged;

        #endregion Events

        public Stopwatch? ExecutionTime;

        #region Privates field

        private Task _executingTask;

        private CancellationTokenSource? _stoppingCts;

        private readonly string _TaskName;

        #endregion Privates field

        #endregion Props


        #region Constructor

        public EventedBackgroundService(string name)
        {
            _TaskName = name;
        }

        #endregion Constructor

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (CurrentlyInProcess())
            {
                SimpleLogger.AddLog("Une tâche est déjà en cours d'exécution.", LogType.Warning);
            }
            else
            {
                try
                {
                    OnTaskStarted();

                    _executingTask = Task.Run(() =>
                    {
                        _stoppingCts = new CancellationTokenSource();

                        PerformTask(_stoppingCts.Token);

                        if (!_stoppingCts.IsCancellationRequested)
                        {
                            OnTaskEnded(true);
                        }

                        return Task.CompletedTask;
                    });
                }
                catch (Exception ex)
                {
                    OnTaskEnded(false);
                }
            }

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }



        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            OnStoped?.Invoke(this, EventArgs.Empty);
            
            SimpleLogger.AddLog("L'utilisateur a stoppé la tâche => " + _TaskName, LogType.Warning);

            // Stop called without start
            if (_executingTask == null)
            {
                SimpleLogger.AddLog("Aucune tâche n'est en cours", LogType.Warning);
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts?.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
                OnTaskEnded(false);
            }

        }

        protected abstract void PerformTask(CancellationToken cancellationToken);


        /// <summary>
        /// Called when the task is ended
        /// </summary>
        /// <param name="hasSucceeded">if set to <c>true</c> [has succeeded].</param>
        protected virtual void OnTaskEnded(bool hasSucceeded)
        {
            ExecutionTime?.Stop();
            SimpleLogger.AddLog(GetEndedMessage(), hasSucceeded ? LogType.Info : LogType.Error);

            ClearContext();

            OnStatusChanged?.Invoke();
        }

        /// <summary>
        /// Called when task as started
        /// </summary>
        protected virtual void OnTaskStarted()
        {
            ExecutionTime = new Stopwatch();
            ExecutionTime.Start();

            OnStarted?.Invoke(this, EventArgs.Empty);
            OnStatusChanged?.Invoke();
            SimpleLogger.AddLog(GetStartedMessage(), LogType.Info);
        }

        protected virtual void ClearContext()
        {
            ExecutionTime = null;
            _stoppingCts = null;
            _executingTask = null;
        }

        protected virtual string GetStartedMessage()
        {
            return "L'utilisateur a débuter la tâche => " + _TaskName;
        }

        protected virtual string GetEndedMessage()
        {
            return "La tâche (" + _TaskName + ") c'est terminer après | " + ExecutionTime?.Elapsed.ToString();
        }

        public bool CurrentlyInProcess()
        {
            return !(_executingTask == null || _executingTask.IsCompleted);
        }
      
        public virtual void Dispose()
        {
            _stoppingCts?.Cancel();
        }
    }
}

