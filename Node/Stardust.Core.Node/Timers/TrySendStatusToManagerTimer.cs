using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Stardust.Core.Node.Extensions;
using Stardust.Core.Node.Interfaces;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using JobDetailSender = Stardust.Core.Node.Workers.JobDetailSender;
//using log4net;
using Timer = System.Timers.Timer;
using LoggerExtensions = Stardust.Core.Node.Extensions.LoggerExtensions;

namespace Stardust.Core.Node.Timers
{
	public class TrySendStatusToManagerTimer : Timer
	{
		private static readonly ILogger Logger = new LoggerFactory().CreateLogger(typeof(TrySendStatusToManagerTimer));
		private readonly CancellationTokenSource _cancellationTokenSource;
		private string _whoAmI;
		private readonly JobDetailSender _jobDetailSender;
		private readonly IHttpSender _httpSender;
		public JobQueueItemEntity JobQueueItemEntity { get; set; }
		public Uri CallbackTemplateUri { get; protected set; }
		public event EventHandler TrySendStatusSucceded;
		protected readonly TimerExceptionLoggerStrategyHandler _exceptionLoggerHandler;
		private bool _enableGc;

		public TrySendStatusToManagerTimer(JobDetailSender jobDetailSender,
		                                   IHttpSender httpSender, double interval = 500) : base(interval)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			//_whoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			//_enableGc = nodeConfiguration.EnableGarbageCollection;
			_jobDetailSender = jobDetailSender;
			_httpSender = httpSender;
			//CallbackTemplateUri = callbackTemplateUri;
			_exceptionLoggerHandler = new TimerExceptionLoggerStrategyHandler(TimerExceptionLoggerStrategyHandler.DefaultLogInterval, GetType());
			
			Elapsed += OnTimedEvent;
			AutoReset = true;
		}

		public virtual void Setup(NodeConfiguration nodeConfiguration, Uri getManagerJobDoneTemplateUri)
		{
			_whoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			_enableGc = nodeConfiguration.EnableGarbageCollection;
			_jobDetailSender.SetManagerLocation(nodeConfiguration.ManagerLocation);
			CallbackTemplateUri = getManagerJobDoneTemplateUri;
		}

		public void InvokeTriggerTrySendStatusSucceded()
		{
			TrySendStatusSucceded?.Invoke(this, EventArgs.Empty);
		}

		protected virtual async Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                                CancellationToken cancellationToken)
		{
			try
			{
				//Use ManagerUriBuilderHelper instead?
				var uri = new Uri(CallbackTemplateUri.ToString().Replace(ManagerRouteConstants.JobIdOptionalParameter,
														jobQueueItemEntity.JobId.ToString()));

				var httpResponseMessage = await _httpSender.PostAsync(uri,
				                                                     null,
				                                                     cancellationToken);
				return httpResponseMessage;
			}

			catch (Exception exp)
			{
				var currentScopeMessage =
					Core.Node.Extensions.LoggerExtensions.GetFormattedLogMessage("Error in TrySendStatus.");
				_exceptionLoggerHandler.LogError(currentScopeMessage, exp);
				throw;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_cancellationTokenSource != null &&
			    !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		private async void OnTimedEvent(object sender,
		                                ElapsedEventArgs e)
		{
			if (JobQueueItemEntity == null)
			{
				return;
			}

			// All job progresses must have been sent to manager. 
			if (_jobDetailSender.DetailsCount() != 0)
			{
				return;
			}

			Stop();

			if (_enableGc)
				GC.Collect();
			try
			{
				var httpResponseMessage = await TrySendStatus(new JobQueueItemEntity
				{
					JobId = JobQueueItemEntity.JobId
				}, _cancellationTokenSource.Token);

				if (httpResponseMessage.IsSuccessStatusCode)
				{
					if(!Logger.IsEnabled(LogLevel.Debug))
					{
						_exceptionLoggerHandler.ResetLastLoggedTime("Sent job status to manager again.");
					}
				
					Logger.DebugWithLineNumber($"{_whoAmI} : Sent job status to manager ({httpResponseMessage.RequestMessage.RequestUri}) " +
											   $"for job ( jobId, jobName ) : ( {JobQueueItemEntity.JobId}, {JobQueueItemEntity.Name} )");
					
					InvokeTriggerTrySendStatusSucceded();
				}
				else
				{
					Start();
					var errorMessage = $"{_whoAmI} : Send status to manager failed for job ( jobId, jobName ) : " +
					                   $"( {JobQueueItemEntity.JobId}, {JobQueueItemEntity.Name} ). Reason : {httpResponseMessage.ReasonPhrase}";
					_exceptionLoggerHandler.LogError(LoggerExtensions.GetFormattedLogMessage(errorMessage));
				}
			}

			catch (Exception exp)
			{
				Start();
				var errorMessage = $"{_whoAmI} : Send status to manager failed for job ( jobId, jobName ) :" +
				                   $" ( {JobQueueItemEntity.JobId}, {JobQueueItemEntity.Name} )";
				_exceptionLoggerHandler.LogError(LoggerExtensions.GetFormattedLogMessage(errorMessage), exp);
			}
		}
	}
}