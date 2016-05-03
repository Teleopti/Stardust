using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Workers
{
	public class WorkerWrapper : IWorkerWrapper
	{
		private const string WorkerIsAlreadyWorking = "Node is already working on another job.";
		private const string JobToDoIsNull = "Job to do can not be null.";
		private const string JobToDoIdIsInvalid = "Job to do property=ID is invalid.";
		private const string JobToDoNameIsInvalid = "Job to do property=NAME is invalid.";
		private const string JobToDoTypeIsNullOrEmpty = "Job to do property=TYPE can not be null or empty string.";
		private const string JobToDoTypeCanNotBeResolved = "Job to do property=TYPE {0}, can not be resolved by container.";
		private const string JobToDoCanNotBeDeserialize = "Job to do property=SERIALIZED can not be deserialized.";

		private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerWrapper));

		private readonly IInvokeHandler _handler;
		private readonly NodeConfiguration _nodeConfiguration;
		private readonly TrySendNodeStartUpNotificationToManagerTimer _nodeStartUpNotificationToManagerTimer;
		private readonly Timer _pingToManagerTimer;

		private readonly object _startJobLock = new object();
		private readonly TrySendStatusToManagerTimer _trySendJobCanceledStatusToManagerTimer;
		private readonly TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
		private readonly TrySendStatusToManagerTimer _trySendJobDoneStatusToManagerTimer;
		private readonly TrySendStatusToManagerTimer _trySendJobFaultedStatusToManagerTimer;
		private JobQueueItemEntity _currentMessageToProcess;
		private TrySendStatusToManagerTimer _trySendStatusToManagerTimer;

		public WorkerWrapper(IInvokeHandler invokeHandler,
		                     NodeConfiguration nodeConfiguration,
		                     TrySendNodeStartUpNotificationToManagerTimer nodeStartUpNotificationToManagerTimer,
		                     Timer pingToManagerTimer,
		                     TrySendJobDoneStatusToManagerTimer trySendJobDoneStatusToManagerTimer,
		                     TrySendJobCanceledToManagerTimer trySendJobCanceledStatusToManagerTimer,
		                     TrySendJobFaultedToManagerTimer trySendJobFaultedStatusToManagerTimer,
		                     TrySendJobDetailToManagerTimer trySendJobDetailToManagerTimer)
		{
			_handler = invokeHandler;
			_nodeConfiguration = nodeConfiguration;
			WhoamI = _nodeConfiguration.CreateWhoIAm(Environment.MachineName);

			_nodeStartUpNotificationToManagerTimer = nodeStartUpNotificationToManagerTimer;
			_nodeStartUpNotificationToManagerTimer.TrySendNodeStartUpNotificationSucceded +=
				NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded;

			_pingToManagerTimer = pingToManagerTimer;
			_trySendJobDoneStatusToManagerTimer = trySendJobDoneStatusToManagerTimer;
			_trySendJobCanceledStatusToManagerTimer = trySendJobCanceledStatusToManagerTimer;
			_trySendJobFaultedStatusToManagerTimer = trySendJobFaultedStatusToManagerTimer;
			_trySendJobDetailToManagerTimer = trySendJobDetailToManagerTimer;

			_trySendJobDetailToManagerTimer.Start();
			_nodeStartUpNotificationToManagerTimer.Start();
		}

		private CancellationTokenSource CurrentMessageTimeoutTaskCancellationTokenSource { get; set; }

		public Task Task { get; private set; }
		public string WhoamI { get; set; }
		public CancellationTokenSource CancellationTokenSource { get; set; }

		public ObjectValidationResult ValidateStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			lock (_startJobLock)
			{
				if (_currentMessageToProcess != null)
				{
					return new ObjectValidationResult
					{
						IsConflict = true,
						Message = WorkerIsAlreadyWorking
					};
				}

				if (jobQueueItemEntity == null)
				{
					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = JobToDoIsNull
					};
				}

				if (jobQueueItemEntity.JobId == Guid.Empty)
				{
					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = JobToDoIdIsInvalid
					};
				}

				if (string.IsNullOrEmpty(jobQueueItemEntity.Name))
				{
					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = JobToDoNameIsInvalid
					};
				}

				if (string.IsNullOrEmpty(jobQueueItemEntity.Type))
				{
					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = JobToDoTypeIsNullOrEmpty
					};
				}

				var type =
					_nodeConfiguration.HandlerAssembly.GetType(jobQueueItemEntity.Type);

				if (type == null)
				{
					Logger.WarningWithLineNumber(string.Format(
						WhoamI +
						": The job type [{0}] could not be resolved. The job cannot be started.",
						jobQueueItemEntity.Type));

					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = string.Format(JobToDoTypeCanNotBeResolved, jobQueueItemEntity.Type)
					};
				}

				try
				{
					JsonConvert.DeserializeObject(jobQueueItemEntity.Serialized, type);
				}
				catch (Exception)
				{
					return new ObjectValidationResult
					{
						IsBadRequest = true,
						Message = JobToDoCanNotBeDeserialize
					};
				}

				_currentMessageToProcess = jobQueueItemEntity;

				return new ObjectValidationResult();
			}
		}

		public void CancelTimeoutCurrentMessageTask()
		{
			if (CurrentMessageTimeoutTaskCancellationTokenSource != null)
			{
				CurrentMessageTimeoutTaskCancellationTokenSource.Cancel();
			}
		}

		public Task CreateTimeoutCurrentMessageTask(JobQueueItemEntity jobQueueItemEntity)
		{
			_currentMessageToProcess = jobQueueItemEntity;

			CurrentMessageTimeoutTaskCancellationTokenSource = new CancellationTokenSource();

			return new Task(() =>
			{
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				while (stopwatch.Elapsed.Seconds <= 60)
				{
					if (IsCancellationRequested)
					{
						CurrentMessageTimeoutTaskCancellationTokenSource.Token.ThrowIfCancellationRequested();
					}

					Thread.Sleep(TimeSpan.FromSeconds(5));
				}

				// Current message timed out.
				_currentMessageToProcess = null;
			}, CurrentMessageTimeoutTaskCancellationTokenSource.Token);
		}

		public void StartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			CancellationTokenSource = new CancellationTokenSource();

			_currentMessageToProcess = jobQueueItemEntity;

			var typ = _nodeConfiguration.HandlerAssembly.GetType(jobQueueItemEntity.Type);
			var deSer = JsonConvert.DeserializeObject(jobQueueItemEntity.Serialized,
			                                          typ);
			//-----------------------------------------------------
			// Clear faulted timer.
			//-----------------------------------------------------
			var faultedTimer =
				_trySendJobFaultedStatusToManagerTimer as TrySendJobFaultedToManagerTimer;

			if (faultedTimer != null)
			{
				faultedTimer.AggregateExceptionToSend = null;
				faultedTimer.ErrorOccured = null;
			}

			//----------------------------------------------------
			// Define task.
			//----------------------------------------------------

			Task = new Task(() =>
			{
				_handler.Invoke(deSer,
				                CancellationTokenSource,
				                SendJobProgressToManager);
			},
			                CancellationTokenSource.Token);

			Task.ContinueWith(t =>
			{
				switch (t.Status)
				{
					case TaskStatus.RanToCompletion:

						SetNodeStatusTimer(_trySendJobDoneStatusToManagerTimer,
						                   _currentMessageToProcess);
						break;


					case TaskStatus.Canceled:

						SetNodeStatusTimer(_trySendJobCanceledStatusToManagerTimer,
						                   _currentMessageToProcess);

						break;


					case TaskStatus.Faulted:
						if (faultedTimer != null)
						{
							faultedTimer.AggregateExceptionToSend = t.Exception;
							faultedTimer.ErrorOccured = DateTime.UtcNow;
						}

						if (t.Exception != null)
						{
							Logger.ErrorWithLineNumber("Failed",
							                           t.Exception);
						}

						SetNodeStatusTimer(_trySendJobFaultedStatusToManagerTimer,
						                   _currentMessageToProcess);

						break;
				}
			}, TaskContinuationOptions.LongRunning);

			Task.Start();
		}

		public JobQueueItemEntity GetCurrentMessageToProcess()
		{
			return _currentMessageToProcess;
		}

		public void CancelJob(Guid id)
		{
			if (_currentMessageToProcess != null &&
			    id != Guid.Empty &&
			    _currentMessageToProcess.JobId == id)
			{
				var token = CancellationTokenSource;
				if (token != null)
				{
					token.Cancel();
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
			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			if (_pingToManagerTimer != null)
			{
				_pingToManagerTimer.Dispose();
			}

			if (_trySendJobCanceledStatusToManagerTimer != null)
			{
				_trySendJobCanceledStatusToManagerTimer.Dispose();
			}

			if (_trySendJobDoneStatusToManagerTimer != null)
			{
				_trySendJobDoneStatusToManagerTimer.Dispose();
			}

			if (_trySendJobFaultedStatusToManagerTimer != null)
			{
				_trySendJobFaultedStatusToManagerTimer.Dispose();
			}

			if (_trySendJobDetailToManagerTimer != null)
			{
				_trySendJobDetailToManagerTimer.Dispose();
			}

			if (_nodeStartUpNotificationToManagerTimer != null)
			{
				_nodeStartUpNotificationToManagerTimer.Dispose();
			}
		}

		private void NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded(object sender,
		                                                                                          EventArgs e)
		{
			_nodeStartUpNotificationToManagerTimer.Stop();
			_pingToManagerTimer.Start();
		}


		public void ResetCurrentMessage()
		{
			_currentMessageToProcess = null;
		}

		private void SetNodeStatusTimer(TrySendStatusToManagerTimer newTrySendStatusToManagerTimer,
		                                JobQueueItemEntity jobQueueItemEntity)
		{
			// Stop and dispose old timer.
			if (_trySendStatusToManagerTimer != null)
			{
				_trySendStatusToManagerTimer.Stop();

				// Remove event handler.
				_trySendStatusToManagerTimer.TrySendStatusSucceded -=
					TrySendStatusToManagerTimer_TrySendStatusSucceded;

				_trySendStatusToManagerTimer = null;
			}

			// Set new timer, if exists.
			if (newTrySendStatusToManagerTimer != null)
			{
				_trySendStatusToManagerTimer = newTrySendStatusToManagerTimer;

				_trySendStatusToManagerTimer.JobQueueItemEntity = jobQueueItemEntity;

				_trySendStatusToManagerTimer.TrySendStatusSucceded +=
					TrySendStatusToManagerTimer_TrySendStatusSucceded;

				_trySendStatusToManagerTimer.Start();
			}
			else
			{
				_trySendStatusToManagerTimer = null;
			}
		}

		private void TrySendStatusToManagerTimer_TrySendStatusSucceded(object sender,
		                                                               EventArgs e)
		{
			// Dispose timer.
			SetNodeStatusTimer(null, null);

			// Clear all job progresses for jobid.
			if (_currentMessageToProcess != null)
			{
				_trySendJobDetailToManagerTimer.ClearAllJobProgresses(_currentMessageToProcess.JobId);
			}

			// Reset jobToDo, so it can start processing new work.
			ResetCurrentMessage();
		}

		private void SendJobProgressToManager(string message)
		{
			if (_currentMessageToProcess != null)
			{
				_trySendJobDetailToManagerTimer.SendProgress(_currentMessageToProcess.JobId,
				                                             message);
			}
		}
	}
}