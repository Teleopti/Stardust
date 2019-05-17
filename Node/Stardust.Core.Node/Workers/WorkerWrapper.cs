using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stardust.Core.Node.Extensions;
using Stardust.Core.Node.Interfaces;
using Stardust.Core.Node.Timers;
using Stardust.Node.Entities;
//using log4net;

namespace Stardust.Core.Node.Workers
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

		private readonly ILogger _logger = new LoggerFactory().CreateLogger(typeof(WorkerWrapper));

		private readonly IInvokeHandler _handler;
		private NodeConfiguration _nodeConfiguration;
		private readonly TrySendNodeStartUpNotificationToManagerTimer _nodeStartUpNotificationToManagerTimer;
		private readonly IPingToManagerTimer _pingToManagerTimer;

		private readonly object _startJobLock = new object();
		private readonly TrySendStatusToManagerTimer _trySendJobCanceledStatusToManagerTimer;
		private readonly TrySendJobDetailToManagerTimer _trySendJobDetailToManagerTimer;
		private readonly TrySendStatusToManagerTimer _trySendJobDoneStatusToManagerTimer;
		private readonly TrySendStatusToManagerTimer _trySendJobFaultedStatusToManagerTimer;
		private JobQueueItemEntity _currentMessageToProcess;
		private TrySendStatusToManagerTimer _trySendStatusToManagerTimer;
		private readonly JobDetailSender _jobDetailSender;
		private readonly INow _now;
		private DateTime? _startJobTimeout;

		public WorkerWrapper(IInvokeHandler invokeHandler,
							 TrySendNodeStartUpNotificationToManagerTimer nodeStartUpNotificationToManagerTimer,
							 IPingToManagerTimer pingToManagerTimer,
							 TrySendJobDoneStatusToManagerTimer trySendJobDoneStatusToManagerTimer,
							 TrySendJobCanceledToManagerTimer trySendJobCanceledStatusToManagerTimer,
							 TrySendJobFaultedToManagerTimer trySendJobFaultedStatusToManagerTimer,
							 TrySendJobDetailToManagerTimer trySendJobDetailToManagerTimer, JobDetailSender jobDetailSender, INow now)
		{
			_handler = invokeHandler;
			_nodeStartUpNotificationToManagerTimer = nodeStartUpNotificationToManagerTimer;
			_nodeStartUpNotificationToManagerTimer.TrySendNodeStartUpNotificationSucceded +=
				NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded;

			_pingToManagerTimer = pingToManagerTimer;
			_trySendJobDoneStatusToManagerTimer = trySendJobDoneStatusToManagerTimer;
			_trySendJobCanceledStatusToManagerTimer = trySendJobCanceledStatusToManagerTimer;
			_trySendJobFaultedStatusToManagerTimer = trySendJobFaultedStatusToManagerTimer;
			_trySendJobDetailToManagerTimer = trySendJobDetailToManagerTimer;
			_jobDetailSender = jobDetailSender;
			_now = now;

			IsWorking = false;
		}

		public void Init(NodeConfiguration nodeConfiguration)
		{
			_jobDetailSender.SetManagerLocation(nodeConfiguration.ManagerLocation);
			_nodeConfiguration = nodeConfiguration;

			_nodeStartUpNotificationToManagerTimer.SetupAndStart(_nodeConfiguration);
			_pingToManagerTimer.SetupAndStart(_nodeConfiguration);

			_trySendJobDoneStatusToManagerTimer.Setup(_nodeConfiguration, nodeConfiguration.GetManagerJobDoneTemplateUri());
			_trySendJobCanceledStatusToManagerTimer.Setup(_nodeConfiguration, nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri());
			_trySendJobFaultedStatusToManagerTimer.Setup(_nodeConfiguration, nodeConfiguration.GetManagerJobHasFailedTemplatedUri());
			_trySendJobDetailToManagerTimer.SetupAndStart(_nodeConfiguration);

			WhoamI = _nodeConfiguration.CreateWhoIAm(Environment.MachineName);
		}

		private CancellationTokenSource CurrentMessageTimeoutTaskCancellationTokenSource { get; set; }

		public Task Task { get; private set; }

		public string WhoamI { get; private set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public ValidateStartJobResult ValidateStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			lock (_startJobLock)
			{
				ResetIfTimeout();
				if (_currentMessageToProcess != null || IsWorking)
				{
					return new ValidateStartJobResult(new HttpResponseMessage(HttpStatusCode.OK), true);
				}
				if (jobQueueItemEntity == null)
				{
					return new ValidateStartJobResult(CreateBadRequest(JobToDoIsNull), false);
				}
				if (jobQueueItemEntity.JobId == Guid.Empty)
				{
					return new ValidateStartJobResult(CreateBadRequest(JobToDoIdIsInvalid), false);
				}
				if (string.IsNullOrEmpty(jobQueueItemEntity.Name))
				{
					return new ValidateStartJobResult( CreateBadRequest(JobToDoNameIsInvalid), false);
				}
				if (string.IsNullOrEmpty(jobQueueItemEntity.Type))
				{
					return new ValidateStartJobResult( CreateBadRequest(JobToDoTypeIsNullOrEmpty), false);
				}
				var type = _nodeConfiguration.HandlerAssembly.GetType(jobQueueItemEntity.Type);

				if (type == null)
				{
					_logger.WarningWithLineNumber(string.Format(
						WhoamI + JobToDoTypeCanNotBeResolved, jobQueueItemEntity.Type));

					return new ValidateStartJobResult( CreateBadRequest(string.Format(JobToDoTypeCanNotBeResolved, jobQueueItemEntity.Type)), false);
				}

				try
				{
					JsonConvert.DeserializeObject(jobQueueItemEntity.Serialized, type);
				}
				catch (Exception)
				{
					return new ValidateStartJobResult( CreateBadRequest(JobToDoCanNotBeDeserialize), false);
				}
				_currentMessageToProcess = jobQueueItemEntity;
				_startJobTimeout = _now.UtcDateTime().AddMinutes(5);
				return new ValidateStartJobResult( new HttpResponseMessage(HttpStatusCode.OK), false);
			}
		}

		private void ResetIfTimeout()
		{
			if (_now.UtcDateTime() >= _startJobTimeout  && IsWorking == false)
			{
				_currentMessageToProcess = null;
				_startJobTimeout = null;
			}

		}

		private static HttpResponseMessage CreateBadRequest(string content)
		{
			var responseMsg = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(content) };
			return responseMsg;
		}

		public void CancelTimeoutCurrentMessageTask()
		{
			CurrentMessageTimeoutTaskCancellationTokenSource?.Cancel();
		}

		public void StartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			if (IsWorking) return;
			IsWorking = true;
			CancellationTokenSource = new CancellationTokenSource();

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
										(message) =>
										{
											_jobDetailSender.AddDetail(_currentMessageToProcess.JobId, message);
										});
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
											  _logger.ErrorWithLineNumber("Failed",
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
				token?.Cancel();
			}
			else
			{
				if (id != Guid.Empty)
				{
					_logger.WarningWithLineNumber(WhoamI + " : Can not cancel job with id : " + id);
				}
			}
		}

		public bool IsCancellationRequested => CancellationTokenSource != null &&
											   CancellationTokenSource.IsCancellationRequested;

		public bool IsWorking { get; private set; }

		public void Dispose()
		{
			if (CancellationTokenSource != null &&
				!CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			_pingToManagerTimer?.Dispose();
			_trySendJobCanceledStatusToManagerTimer?.Dispose();
			_trySendJobDoneStatusToManagerTimer?.Dispose();
			_trySendJobFaultedStatusToManagerTimer?.Dispose();
			_trySendJobDetailToManagerTimer?.Dispose();
			_nodeStartUpNotificationToManagerTimer?.Dispose();
		}

		private void NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded(object sender,
																								  EventArgs e)
		{
			_nodeStartUpNotificationToManagerTimer.Stop();
			_nodeStartUpNotificationToManagerTimer.TrySendNodeStartUpNotificationSucceded -=
				NodeStartUpNotificationToManagerTimer_TrySendNodeStartUpNotificationSucceded;
			_pingToManagerTimer.SetupAndStart(_nodeConfiguration);
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
					TrySendStatusToManagerTimer_TrySendStatus;

				_trySendStatusToManagerTimer = null;
			}

			// Set new timer, if exists.
			if (newTrySendStatusToManagerTimer != null)
			{
				_trySendStatusToManagerTimer = newTrySendStatusToManagerTimer;
				_trySendStatusToManagerTimer.JobQueueItemEntity = jobQueueItemEntity;

				_trySendStatusToManagerTimer.TrySendStatusSucceded +=
					TrySendStatusToManagerTimer_TrySendStatus;

				_trySendStatusToManagerTimer.Start();
			}
			else
			{
				if(_trySendStatusToManagerTimer != null)
				{
					_trySendStatusToManagerTimer.Stop();
					_trySendStatusToManagerTimer.TrySendStatusSucceded -=
						TrySendStatusToManagerTimer_TrySendStatus;

					_trySendStatusToManagerTimer = null;
				}
			}
		}

		private void TrySendStatusToManagerTimer_TrySendStatus(object sender,
																	   EventArgs e)
		{
			_trySendStatusToManagerTimer.Stop();
			_trySendStatusToManagerTimer.TrySendStatusSucceded -=
				TrySendStatusToManagerTimer_TrySendStatus;

			_trySendStatusToManagerTimer = null;

			// Dispose timer.
			_currentMessageToProcess = null;
			IsWorking = false;
		}
	}
}