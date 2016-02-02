using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Workers
{
    public class WorkerWrapper : IWorkerWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerWrapper));

        private readonly IPostHttpRequest _postHttpRequest;

        private readonly object _startJobLock = new object();

        public WorkerWrapper(IInvokeHandler invokeHandler,
                             INodeConfiguration nodeConfiguration,
                             TrySendNodeStartUpNotificationToManagerTimer nodeStartUpNotificationToManagerTimer,
                             Timer pingToManagerTimer,
                             TrySendStatusToManagerTimer trySendJobDoneStatusToManagerTimer,
                             TrySendStatusToManagerTimer trySendJobCanceledStatusToManagerTimer,
                             TrySendStatusToManagerTimer trySendJobFaultedStatusToManagerTimer,
                             IPostHttpRequest postHttpRequest)
        {
            _postHttpRequest = postHttpRequest;

            invokeHandler.ThrowArgumentNullExceptionWhenNull();
            nodeConfiguration.ThrowArgumentNullException();

            nodeStartUpNotificationToManagerTimer.ThrowArgumentNullExceptionWhenNull();
            pingToManagerTimer.ThrowArgumentNullExceptionWhenNull();
            trySendJobDoneStatusToManagerTimer.ThrowArgumentNullExceptionWhenNull();
            trySendJobCanceledStatusToManagerTimer.ThrowArgumentNullExceptionWhenNull();
            trySendJobFaultedStatusToManagerTimer.ThrowArgumentNullExceptionWhenNull();

            Handler = invokeHandler;
            NodeConfiguration = nodeConfiguration;

            WhoamI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

            NodeStartUpNotificationToManagerTimer = nodeStartUpNotificationToManagerTimer;
            NodeStartUpNotificationToManagerTimer.TrySendNodeStartUpNotificationSucceded +=
                NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded;

            PingToManagerTimer = pingToManagerTimer;

            TrySendJobDoneStatusToManagerTimer = trySendJobDoneStatusToManagerTimer;
            TrySendJobCanceledStatusToManagerTimer = trySendJobCanceledStatusToManagerTimer;
            TrySendJobFaultedStatusToManagerTimer = trySendJobFaultedStatusToManagerTimer;

            NodeStartUpNotificationToManagerTimer.Start();
        }

        private IInvokeHandler Handler { get; set; }
        private Timer PingToManagerTimer { get; set; }
        private TrySendStatusToManagerTimer TrySendJobDoneStatusToManagerTimer { get; set; }
        private TrySendStatusToManagerTimer TrySendJobCanceledStatusToManagerTimer { get; set; }
        private TrySendStatusToManagerTimer TrySendJobFaultedStatusToManagerTimer { get; set; }

        private TrySendStatusToManagerTimer TrySendStatusToManagerTimer { get; set; }
        private INodeConfiguration NodeConfiguration { get; set; }
        private JobToDo CurrentMessageToProcess { get; set; }
        private TrySendNodeStartUpNotificationToManagerTimer NodeStartUpNotificationToManagerTimer { get; set; }

        private bool SendNodeStartUpNotificationSucceded { get; set; }

        public bool TryCloseDown()
        {
            NodeStartUpNotificationToManagerTimer.Stop();
            PingToManagerTimer.Stop();            
            TrySendJobDoneStatusToManagerTimer.Stop();
            TrySendJobCanceledStatusToManagerTimer.Stop();
            TrySendJobFaultedStatusToManagerTimer.Stop();

            return true;
        }

        public string WhoamI { get; private set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Task Task { get; private set; }

        public bool IsTaskExecuting
        {
            get
            {
                return Task.IsNotNull() &&
                       (Task.Status == TaskStatus.Running || Task.Status == TaskStatus.WaitingForActivation);
            }
        }

        public IHttpActionResult StartJob(JobToDo jobToDo,
                                          HttpRequestMessage requestMessage)
        {
            Type typ;
            lock (_startJobLock)
            {
                if (jobToDo == null || jobToDo.Id == Guid.Empty)
                {
                    return new BadRequestResult(requestMessage);
                }

                if (CurrentMessageToProcess != null &&
                    CurrentMessageToProcess.Id != jobToDo.Id)
                {
                    return new ConflictResult(requestMessage);
                }

                typ = NodeConfiguration.HandlerAssembly.GetType(jobToDo.Type);

                if (typ == null)
                {
                    Logger.Info(string.Format(WhoamI + ": The type [{0}] could not be resolved. The job cannot be started.",
                                              jobToDo.Type));

                    return new BadRequestResult(requestMessage);
                }

                CurrentMessageToProcess = jobToDo;
            }

            CancellationTokenSource = new CancellationTokenSource();
            object deSer;
            try
            {
                deSer = JsonConvert.DeserializeObject(jobToDo.Serialized,
                                                      typ);
            }
            catch (Exception)
            {
                CurrentMessageToProcess = null;
                return new BadRequestResult(requestMessage);
            }
            if (deSer == null)
            {
                return new BadRequestResult(requestMessage);
            }

            //----------------------------------------------------
            // Define task.
            //----------------------------------------------------
            Task = new Task(() =>
            {
                Handler.Invoke(deSer,
                               CancellationTokenSource,
                               ProgressCallback);
            },
                            CancellationTokenSource.Token);

            Task.ContinueWith(t =>
            {
                string logInfo = null;

                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        logInfo =
                            string.Format("{0} : The task has completed for (id, name) : ({1}, {2})",
                                          WhoamI,
                                          CurrentMessageToProcess.Id,
                                          CurrentMessageToProcess.Name);

                        Logger.Info(logInfo);

                        SetNodeStatusTimer(TrySendJobDoneStatusToManagerTimer,
                                           CurrentMessageToProcess);
                        break;


                    case TaskStatus.Canceled:
                        logInfo =
                            string.Format("{0} : The task has been canceled for (id, name) : ({1}, {2})",
                                          WhoamI,
                                          CurrentMessageToProcess.Id,
                                          CurrentMessageToProcess.Name);

                        Logger.Info(logInfo);

                        SetNodeStatusTimer(TrySendJobCanceledStatusToManagerTimer,
                                           CurrentMessageToProcess);

                        break;

                    case TaskStatus.Faulted:
                        logInfo =
                            string.Format("{0} : The task faulted for (id, name) : ({1}, {2})",
                                          WhoamI,
                                          CurrentMessageToProcess.Id,
                                          CurrentMessageToProcess.Name);

                        Logger.Info(logInfo);


                        SetNodeStatusTimer(TrySendJobFaultedStatusToManagerTimer,
                                           CurrentMessageToProcess);

                        break;
                }
            });

            Task.Start();

            return new OkResult(requestMessage);
        }

        public JobToDo GetCurrentMessageToProcess()
        {
            return CurrentMessageToProcess;
        }

        public void CancelJob(Guid id)
        {
            if (CurrentMessageToProcess != null &&
                id != Guid.Empty &&
                CurrentMessageToProcess.Id == id)
            {
                Logger.Info(WhoamI + " : Cancel job method called. Will call cancel on canellation token source.");

                CancellationTokenSource.Cancel();

                if (CancellationTokenSource.IsCancellationRequested)
                {
                    Logger.Info(WhoamI + " : Cancel job method called. CancellationTokenSource.IsCancellationRequested is now true.");
                }
            }
            else
            {
                if (id != Guid.Empty)
                {
                    Logger.Warn(WhoamI + " : Can not cancel job with id : " + id);
                }
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                return CancellationTokenSource != null &&
                       CancellationTokenSource.IsCancellationRequested;
            }
        }

        private void NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded(object sender,
                                                                                                  EventArgs e)
        {
            SendNodeStartUpNotificationSucceded = true;
            NodeStartUpNotificationToManagerTimer.Stop();

            PingToManagerTimer.Start();
        }

        public void ResetCurrentMessage()
        {
            CurrentMessageToProcess = null;
        }

        private void SetNodeStatusTimer(TrySendStatusToManagerTimer newTrySendStatusToManagerTimer,
                                        JobToDo jobToDo)
        {
            // Stop and dispose old timer.
            if (TrySendStatusToManagerTimer != null)
            {
                TrySendStatusToManagerTimer.Stop();

                // Remove event handler.
                TrySendStatusToManagerTimer.TrySendStatusSucceded -= TrySendStatusToManagerTimer_TrySendStatusSucceded;

                TrySendStatusToManagerTimer = null;
            }

            // Set new timer, if exists.
            if (newTrySendStatusToManagerTimer != null)
            {
                TrySendStatusToManagerTimer = newTrySendStatusToManagerTimer;

                TrySendStatusToManagerTimer.JobToDo = jobToDo;

                TrySendStatusToManagerTimer.TrySendStatusSucceded += TrySendStatusToManagerTimer_TrySendStatusSucceded;

                TrySendStatusToManagerTimer.Start();
            }
            else
            {
                TrySendStatusToManagerTimer = null;
            }
        }

        private void TrySendStatusToManagerTimer_TrySendStatusSucceded(object sender,
                                                                       EventArgs e)
        {
            // Dispose timer.
            SetNodeStatusTimer(null,
                               null);

            // Reset jobToDo, so it can start processing new work.
            ResetCurrentMessage();
        }

        private void ProgressCallback(string message)
        {
            Logger.Info(message);

            var progressModel = new JobProgressModel
            {
                JobId = CurrentMessageToProcess.Id,
                ProgressDetail = message
            };

            var json = JsonConvert.SerializeObject(progressModel);

            try
            {
                _postHttpRequest.Send<IHttpActionResult>(NodeConfiguration.ManagerLocation + ManagerRouteConstants.JobProgress,
                                                         json);
            }
            catch (Exception exception)
            {
                Logger.Error(WhoamI + ": Exception occured.",
                             exception);
            }
        }
    }
}