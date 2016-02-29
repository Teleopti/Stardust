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
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Workers
{
	public class WorkerWrapper : IWorkerWrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerWrapper));

		private readonly IHttpSender _httpSender;

		private readonly object _startJobLock = new object();

		public WorkerWrapper(IInvokeHandler invokeHandler,
		                     INodeConfiguration nodeConfiguration,
		                     TrySendNodeStartUpNotificationToManagerTimer nodeStartUpNotificationToManagerTimer,
		                     Timer pingToManagerTimer,
		                     TrySendStatusToManagerTimer trySendJobDoneStatusToManagerTimer,
		                     TrySendStatusToManagerTimer trySendJobCanceledStatusToManagerTimer,
		                     TrySendStatusToManagerTimer trySendJobFaultedStatusToManagerTimer,
		                     IHttpSender httpSender)
		{
			_httpSender = httpSender;

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
				if (string.IsNullOrEmpty(jobToDo.Type))
				{
					return new BadRequestResult(requestMessage);
				}

				typ = NodeConfiguration.HandlerAssembly.GetType(jobToDo.Type);

				if (typ == null)
				{
					LogHelper.LogWarningWithLineNumber(Logger,
					                                   string.Format(
						                                   WhoamI +
						                                   ": The job type [{0}] could not be resolved. The job cannot be started.",
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
				PingToManagerTimer.Interval = NodeConfiguration.PingToManagerRunningDelay*1000; //milliseconds

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Ping to manager interval is now set to go every " +
				                                 PingToManagerTimer.Interval + " seconds during job execution.");

				Handler.Invoke(deSer,
				               CancellationTokenSource,
				               ProgressCallback);
			},
			                CancellationTokenSource.Token);

			Task.ContinueWith(t =>
			{
				PingToManagerTimer.Interval = NodeConfiguration.PingToManagerIdleDelay*1000; //milliseconds

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Ping to manager interval is now set to go every " +
				                                 PingToManagerTimer.Interval + " seconds when node is idle.");


				string logInfo;

				switch (t.Status)
				{
					case TaskStatus.RanToCompletion:
						logInfo =
							string.Format("{0} : The task has completed for job ( jobId, jobName ) : ( {1}, {2} )",
							              WhoamI,
							              CurrentMessageToProcess.Id,
							              CurrentMessageToProcess.Name);

						LogHelper.LogDebugWithLineNumber(Logger,
						                                 logInfo);

						SetNodeStatusTimer(TrySendJobDoneStatusToManagerTimer,
						                   CurrentMessageToProcess);
						break;


					case TaskStatus.Canceled:
						logInfo =
							string.Format("{0} : The task has been canceled for job ( jobId, jobName ) : ( {1}, {2} )",
							              WhoamI,
							              CurrentMessageToProcess.Id,
							              CurrentMessageToProcess.Name);

						LogHelper.LogDebugWithLineNumber(Logger,
						                                 logInfo);

						SetNodeStatusTimer(TrySendJobCanceledStatusToManagerTimer,
						                   CurrentMessageToProcess);

						break;


					case TaskStatus.Faulted:
						logInfo =
							string.Format("{0} : The task faulted for job ( jobId, jobName ) : ( {1}, {2} )",
							              WhoamI,
							              CurrentMessageToProcess.Id,
							              CurrentMessageToProcess.Name);

						if (t.Exception != null)
						{
							foreach (var e in t.Exception.InnerExceptions)
							{
								LogHelper.LogErrorWithLineNumber(Logger,
								                                 logInfo, e);
							}
						}

						SetNodeStatusTimer(TrySendJobFaultedStatusToManagerTimer,
						                   CurrentMessageToProcess);

						break;
				}
			}, TaskContinuationOptions.LongRunning);

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
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 WhoamI +
				                                 " : Cancel job method called. Will call cancel on canellation token source.");

				CancellationTokenSource.Cancel();

				if (CancellationTokenSource.IsCancellationRequested)
				{
					LogHelper.LogDebugWithLineNumber(Logger,
					                                 WhoamI +
					                                 " : Cancel job method called. CancellationTokenSource.IsCancellationRequested is now true.");
				}
			}
			else
			{
				if (id != Guid.Empty)
				{
					LogHelper.LogWarningWithLineNumber(Logger,
					                                   WhoamI + " : Can not cancel job with id : " + id);
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

		public void Dispose()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			if (PingToManagerTimer != null)
			{
				PingToManagerTimer.Dispose();
			}

			if (TrySendJobCanceledStatusToManagerTimer != null)
			{
				TrySendJobCanceledStatusToManagerTimer.Dispose();
			}

			if (TrySendJobDoneStatusToManagerTimer != null)
			{
				TrySendJobDoneStatusToManagerTimer.Dispose();
			}

			if (TrySendJobFaultedStatusToManagerTimer != null)
			{
				TrySendJobFaultedStatusToManagerTimer.Dispose();
			}

			if (NodeStartUpNotificationToManagerTimer != null)
			{
				NodeStartUpNotificationToManagerTimer.Dispose();
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
		}

		private void NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded(object sender,
		                                                                                          EventArgs e)
		{
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
				TrySendStatusToManagerTimer.TrySendStatusSucceded -=
					TrySendStatusToManagerTimer_TrySendStatusSucceded;

				TrySendStatusToManagerTimer = null;
			}

			// Set new timer, if exists.
			if (newTrySendStatusToManagerTimer != null)
			{
				TrySendStatusToManagerTimer = newTrySendStatusToManagerTimer;

				TrySendStatusToManagerTimer.JobToDo = jobToDo;

				TrySendStatusToManagerTimer.TrySendStatusSucceded +=
					TrySendStatusToManagerTimer_TrySendStatusSucceded;

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
			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoamI + " : " + message);

			var progressModel = new JobProgressModel
			{
				JobId = CurrentMessageToProcess.Id,
				ProgressDetail = message
			};

			try
			{
				var uriBuilder =
					new UriBuilder(NodeConfiguration.ManagerLocation);

				uriBuilder.Path += ManagerRouteConstants.JobProgress;

				_httpSender.PostAsync(uriBuilder.Uri, progressModel);
			}

			catch (Exception exception)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 WhoamI + " : Exception occured.",
				                                 exception);
			}
		}
	}
}