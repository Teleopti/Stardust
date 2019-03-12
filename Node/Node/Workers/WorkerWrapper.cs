using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.ReturnObjects;
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

		private static readonly ILog Logger = LogManager.GetLogger(typeof(WorkerWrapper));

		private readonly IInvokeHandler _handler;
		//private readonly NodeConfigurationService _nodeConfigurationService;
		private NodeConfiguration _nodeConfiguration;
		private readonly TrySendNodeStartUpNotificationToManagerTimer _nodeStartUpNotificationToManagerTimer;
		private readonly PingToManagerTimer _pingToManagerTimer;

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
							 //NodeConfiguration nodeConfiguration,
							 //NodeConfigurationService nodeConfigurationService,
							 TrySendNodeStartUpNotificationToManagerTimer nodeStartUpNotificationToManagerTimer,
							 PingToManagerTimer pingToManagerTimer,
							 TrySendJobDoneStatusToManagerTimer trySendJobDoneStatusToManagerTimer,
							 TrySendJobCanceledToManagerTimer trySendJobCanceledStatusToManagerTimer,
							 TrySendJobFaultedToManagerTimer trySendJobFaultedStatusToManagerTimer,
							 TrySendJobDetailToManagerTimer trySendJobDetailToManagerTimer, JobDetailSender jobDetailSender, INow now)
		{
			_handler = invokeHandler;
			//_nodeConfigurationService = nodeConfigurationService;
			//_nodeConfiguration = nodeConfiguration;
			//WhoamI = _nodeConfiguration.CreateWhoIAm(Environment.MachineName);

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

			//_trySendJobDetailToManagerTimer.Start();
			//_nodeStartUpNotificationToManagerTimer.Start();
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

		public HttpResponseMessage ValidateStartJob(JobQueueItemEntity jobQueueItemEntity)
		{
			lock (_startJobLock)
			{
				resetIfTimeout();
				if (_currentMessageToProcess != null || IsWorking)
				{
					var responseMsg = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent(WorkerIsAlreadyWorking) };
					return responseMsg;
				}
				if (jobQueueItemEntity == null)
				{
					return CreateBadRequest(JobToDoIsNull);
				}
				if (jobQueueItemEntity.JobId == Guid.Empty)
				{
					return CreateBadRequest(JobToDoIdIsInvalid);
				}
				if (string.IsNullOrEmpty(jobQueueItemEntity.Name))
				{
					return CreateBadRequest(JobToDoNameIsInvalid);
				}
				if (string.IsNullOrEmpty(jobQueueItemEntity.Type))
				{
					return CreateBadRequest(JobToDoTypeIsNullOrEmpty);
				}
				var type = _nodeConfiguration.HandlerAssembly.GetType(jobQueueItemEntity.Type);

				if (type == null)
				{
					Logger.WarningWithLineNumber(string.Format(
						WhoamI + JobToDoTypeCanNotBeResolved, jobQueueItemEntity.Type));

					return CreateBadRequest(string.Format(JobToDoTypeCanNotBeResolved, jobQueueItemEntity.Type));
				}

				try
				{
					JsonConvert.DeserializeObject(jobQueueItemEntity.Serialized, type);
				}
				catch (Exception)
				{
					return CreateBadRequest(JobToDoCanNotBeDeserialize);
				}
				_currentMessageToProcess = jobQueueItemEntity;
				_startJobTimeout = _now.UtcDateTime().AddMinutes(5);
				return new HttpResponseMessage(HttpStatusCode.OK);
			}
		}

		private void resetIfTimeout()
		{
			if (_now.UtcDateTime() >= _startJobTimeout)
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
			IEnumerable<object> returnObjects = null;

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
												SendJobProgressToManager,
												ref returnObjects);
							},
							CancellationTokenSource.Token);

			Task.ContinueWith(t =>
							  {
								  switch (t.Status)
								  {
									  case TaskStatus.RanToCompletion:

										  SetNodeStatusTimer(_trySendJobDoneStatusToManagerTimer,
															 _currentMessageToProcess);

										  if (returnObjects != null)
										  {
											  SpinWait.SpinUntil(() => _currentMessageToProcess == null);
											  LoopReturnObjects(returnObjects);
										  }

										  break;


									  case TaskStatus.Canceled:

										  SetNodeStatusTimer(_trySendJobCanceledStatusToManagerTimer,
															 _currentMessageToProcess);

										  if (returnObjects != null)
										  {
											  SpinWait.SpinUntil(() => _currentMessageToProcess == null);
											  LoopReturnObjects(returnObjects);
										  }

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

										  if (returnObjects != null)
										  {
											  SpinWait.SpinUntil(() => _currentMessageToProcess == null);
											  LoopReturnObjects(returnObjects);
										  }

										  break;
								  }
							  }, TaskContinuationOptions.LongRunning);

			Task.Start();
		}

		private void LoopReturnObjects(IEnumerable<object> returnObjects)
		{
			if (returnObjects == null) return;

			foreach (var returnObject in returnObjects)
			{
				if (returnObject is ExitApplication)
				{
					EnvironmentExit(returnObject as ExitApplication);
;				}
			}
		}

		private void EnvironmentExit(ExitApplication exitApplication)
		{
			Environment.Exit(exitApplication.ExitCode);
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
					Logger.WarningWithLineNumber(WhoamI + " : Can not cancel job with id : " + id);
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
				//_trySendStatusToManagerTimer.Setup(_nodeConfiguration, _nodeConfiguration.GetManagerNodeHeartbeatUri());
				_trySendStatusToManagerTimer.JobQueueItemEntity = jobQueueItemEntity;

				_trySendStatusToManagerTimer.TrySendStatusSucceded +=
					TrySendStatusToManagerTimer_TrySendStatus;

				_trySendStatusToManagerTimer.Start();
			}
			else
			{
				_trySendStatusToManagerTimer?.Dispose();
				_trySendStatusToManagerTimer = null;
			}
		}

		private void TrySendStatusToManagerTimer_TrySendStatus(object sender,
																	   EventArgs e)
		{
			// Dispose timer.
			SetNodeStatusTimer(null, null);
			_currentMessageToProcess = null;
			IsWorking = false;
		}

		private void SendJobProgressToManager(string message)
		{
			_jobDetailSender.AddDetail(_currentMessageToProcess.JobId, message);
		}
	}
}