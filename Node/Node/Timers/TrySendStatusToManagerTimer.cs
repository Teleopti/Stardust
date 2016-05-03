using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendStatusToManagerTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (TrySendStatusToManagerTimer));

		public TrySendStatusToManagerTimer(NodeConfiguration nodeConfiguration,
		                                   Uri callbackTemplateUri,
		                                   TrySendJobDetailToManagerTimer sendJobDetailToManagerTimer,
		                                   IHttpSender httpSender, double interval = 500) : base(interval)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_whoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			CallbackTemplateUri = callbackTemplateUri;
			_sendJobDetailToManagerTimer = sendJobDetailToManagerTimer;
			_httpSender = httpSender;

			Elapsed += OnTimedEvent;
			AutoReset = true;
		}

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly string _whoAmI;
		private readonly TrySendJobDetailToManagerTimer _sendJobDetailToManagerTimer;
		private readonly IHttpSender _httpSender;

		public JobQueueItemEntity JobQueueItemEntity { get; set; }
		public Uri CallbackTemplateUri { get; set; }
		public event EventHandler TrySendStatusSucceded;

		public void InvokeTriggerTrySendStatusSucceded()
		{
			if (TrySendStatusSucceded != null)
			{
				TrySendStatusSucceded(this, EventArgs.Empty);
			}
		}

		protected virtual async Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                                CancellationToken cancellationToken)
		{
			try
			{
				var uri = jobQueueItemEntity.CreateUri(CallbackTemplateUri.ToString());

				var httpResponseMessage = await _httpSender.PostAsync(uri,
				                                                     null,
				                                                     cancellationToken);
				return httpResponseMessage;
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber("Error in TrySendStatus.", exp);
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
			if (!_sendJobDetailToManagerTimer.HasAllProgressesBeenSent(JobQueueItemEntity.JobId))
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
					var msg = string.Format("{0} : Sent job status to manager ({3}) for job ( jobId, jobName ) : ( {1}, {2} )",
					                        _whoAmI,
					                        JobQueueItemEntity.JobId,
					                        JobQueueItemEntity.Name,
					                        httpResponseMessage.RequestMessage.RequestUri);

					Logger.DebugWithLineNumber(msg);


					InvokeTriggerTrySendStatusSucceded();
				}
				else
				{
					Start();

					var msg =
						string.Format("{0} : Send status to manager failed for job ( jobId, jobName ) : ( {1}, {2} ). Reason : {3}",
						              _whoAmI,
						              JobQueueItemEntity.JobId,
						              JobQueueItemEntity.Name,
						              httpResponseMessage.ReasonPhrase);

					Logger.InfoWithLineNumber(msg);
				}
			}

			catch (Exception exp)
			{
				Start();

				var msg =
					string.Format("{0} : Send status to manager failed for job ( jobId, jobName ) : ( {1}, {2} )",
					              _whoAmI,
					              JobQueueItemEntity.JobId,
					              JobQueueItemEntity.Name);

				Logger.ErrorWithLineNumber(msg, exp);
			}
		}
	}
}