﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendStatusToManagerTimer : Timer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(TrySendStatusToManagerTimer));
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly string _whoAmI;
		private readonly JobDetailSender _jobDetailSender;
		private readonly IHttpSender _httpSender;
		public JobQueueItemEntity JobQueueItemEntity { get; set; }
		public Uri CallbackTemplateUri { get; set; }
		public event EventHandler TrySendStatusSucceded;
		protected readonly TimerExceptionLoggerStrategyHandler _exceptionLoggerHandler;

		public TrySendStatusToManagerTimer(NodeConfiguration nodeConfiguration,
		                                   Uri callbackTemplateUri,
		                                   JobDetailSender jobDetailSender,
		                                   IHttpSender httpSender, double interval = 500) : base(interval)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_whoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			_jobDetailSender = jobDetailSender;
			_httpSender = httpSender;
			CallbackTemplateUri = callbackTemplateUri;
			_exceptionLoggerHandler = new TimerExceptionLoggerStrategyHandler(TimerExceptionLoggerStrategyHandler.DefaultLogInterval, GetType());
			
			Elapsed += OnTimedEvent;
			AutoReset = true;
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
					LoggerExtensions.GetFormattedLogMessage("Error in TrySendStatus.");
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

			try
			{
				var httpResponseMessage = await TrySendStatus(new JobQueueItemEntity
				{
					JobId = JobQueueItemEntity.JobId
				}, _cancellationTokenSource.Token);

				if (httpResponseMessage.IsSuccessStatusCode)
				{
					if(!Logger.IsDebugEnabled)
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