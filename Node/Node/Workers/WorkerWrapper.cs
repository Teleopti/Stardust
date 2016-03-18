using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Autofac.Extras.DynamicProxy2;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Diagnostics;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net;
using Stardust.Node.Log4Net.Extensions;
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
		                     TrySendJobProgressToManagerTimer trySendJobProgressToManagerTimer,
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
			trySendJobProgressToManagerTimer.ThrowArgumentNullExceptionWhenNull();

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

			TrySendJobProgressToManagerTimer = trySendJobProgressToManagerTimer;
			TrySendJobProgressToManagerTimer.Start();

			NodeStartUpNotificationToManagerTimer.Start();
		}

		private IInvokeHandler Handler { get; set; }
		private Timer PingToManagerTimer { get; set; }
		private TrySendStatusToManagerTimer TrySendJobDoneStatusToManagerTimer { get; set; }
		private TrySendStatusToManagerTimer TrySendJobCanceledStatusToManagerTimer { get; set; }
		private TrySendStatusToManagerTimer TrySendJobFaultedStatusToManagerTimer { get; set; }
		private TrySendJobProgressToManagerTimer TrySendJobProgressToManagerTimer { get; set; }

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

		public IHttpActionResult ValidateStartJob(JobToDo jobToDo,
		                                          HttpRequestMessage requestMessage)
		{
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

				var typ = NodeConfiguration.HandlerAssembly.GetType(jobToDo.Type);

				if (typ == null)
				{
					Logger.WarningWithLineNumber(string.Format(
						WhoamI +
						": The job type [{0}] could not be resolved. The job cannot be started.",
						jobToDo.Type));

					return new BadRequestResult(requestMessage);
				}

				CurrentMessageToProcess = jobToDo;

				return new OkResult(requestMessage);
			}
		}

		public IHttpActionResult StartJob(JobToDo jobToDo,
		                                  HttpRequestMessage requestMessage)
		{
			CancellationTokenSource = new CancellationTokenSource();
			object deSer;
			try
			{
				var typ = NodeConfiguration.HandlerAssembly.GetType(jobToDo.Type);
				deSer = JsonConvert.DeserializeObject(jobToDo.Serialized,
				                                      typ);
				CurrentMessageToProcess = jobToDo;
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

			//-----------------------------------------------------
			// Clear faulted timer.
			//-----------------------------------------------------
			var faultedTimer =
				TrySendJobFaultedStatusToManagerTimer as TrySendJobFaultedToManagerTimer;

			if (faultedTimer != null)
			{
				faultedTimer.AggregateExceptionToSend = null;
				faultedTimer.ErrorOccured = null;
			}

			//----------------------------------------------------
			// Define task.
			//----------------------------------------------------
			var taskToExecuteStopWatch = new TaskToExecuteStopWatch(false);

			Task = new Task(() =>
			{
				taskToExecuteStopWatch.Start();

				Handler.Invoke(deSer,
				               CancellationTokenSource,
				               SendJobProgressToManager);
			},
			                CancellationTokenSource.Token);

			Task.ContinueWith(t =>
			{
				Logger.DebugWithLineNumber(string.Format(
					"Job ( id, name, type ) : ( {0}, {1}, {2} ) took ( seconds, minutes ) : ( {3}, {4} )",
					CurrentMessageToProcess.Id,
					CurrentMessageToProcess.Name,
					CurrentMessageToProcess.Type,
					taskToExecuteStopWatch.GetTotalElapsedTimeInSeconds(),
					taskToExecuteStopWatch.GetTotalElapsedTimeInMinutes()));

				string logInfo;

				switch (t.Status)
				{
					case TaskStatus.RanToCompletion:
						logInfo =
							string.Format("{0} : The task has completed for job ( jobId, jobName ) : ( {1}, {2} )",
							              WhoamI,
							              CurrentMessageToProcess.Id,
							              CurrentMessageToProcess.Name);

						Logger.DebugWithLineNumber(logInfo);

						SetNodeStatusTimer(TrySendJobDoneStatusToManagerTimer,
						                   CurrentMessageToProcess);
						break;


					case TaskStatus.Canceled:
						logInfo =
							string.Format("{0} : The task has been canceled for job ( jobId, jobName ) : ( {1}, {2} )",
							              WhoamI,
							              CurrentMessageToProcess.Id,
							              CurrentMessageToProcess.Name);

						Logger.DebugWithLineNumber(logInfo);

						SetNodeStatusTimer(TrySendJobCanceledStatusToManagerTimer,
						                   CurrentMessageToProcess);

						break;


					case TaskStatus.Faulted:
						if (faultedTimer != null)
						{
							faultedTimer.AggregateExceptionToSend = t.Exception;
							faultedTimer.ErrorOccured = DateTime.Now;
						}

						if (t.Exception != null)
						{
							foreach (var exp in t.Exception.InnerExceptions)
							{
								var errorMessage =
									string.Format("( Message, Source, StackTrace ): ( {0}, {1}, {2} )",
									              exp.InnerException.Message,
									              exp.InnerException.Source,
									              exp.InnerException.StackTrace);

								Logger.ErrorWithLineNumber(errorMessage, exp);
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
				Logger.DebugWithLineNumber(WhoamI +
				                           " : Cancel job method called. Will call cancel on canellation token source.");

				CancellationTokenSource.Cancel();

				if (CancellationTokenSource.IsCancellationRequested)
				{
					Logger.DebugWithLineNumber(WhoamI +
					                           " : Cancel job method called. CancellationTokenSource.IsCancellationRequested is now true.");
				}
			}
			else
			{
				if (id != Guid.Empty)
				{
					Logger.WarningWithLineNumber(WhoamI + " : Can not cancel job with id : " + id);
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
			Logger.DebugWithLineNumber("Start disposing.");

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

			if (TrySendJobProgressToManagerTimer != null)
			{
				TrySendJobProgressToManagerTimer.Dispose();
			}

			if (NodeStartUpNotificationToManagerTimer != null)
			{
				NodeStartUpNotificationToManagerTimer.Dispose();
			}


			Logger.DebugWithLineNumber("Finished disposing.");
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

			// Clear all job progresses for jobid.
			if (CurrentMessageToProcess != null)
			{
				TrySendJobProgressToManagerTimer.ClearAllJobProgresses(CurrentMessageToProcess.Id);
			}

			// Reset jobToDo, so it can start processing new work.
			ResetCurrentMessage();
		}

		private void SendJobProgressToManager(string message)
		{
			if (CurrentMessageToProcess != null)
			{
				TrySendJobProgressToManagerTimer.SendProgress(CurrentMessageToProcess.Id,
				                                              message);
			}
		}
	}
}