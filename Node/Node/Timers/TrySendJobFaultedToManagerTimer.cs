using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobFaultedToManagerTimer));

		public TrySendJobFaultedToManagerTimer(NodeConfiguration nodeConfiguration,
		                                       TrySendJobDetailToManagerTimer sendJobDetailToManagerTimer,
											   IHttpSender httpSender,
		                                       double interval = 500) : base(nodeConfiguration,
		                                                                      nodeConfiguration
			                                                                      .GetManagerJobHasFaileTemplatedUri(),
		                                                                      sendJobDetailToManagerTimer,
																			  httpSender,
																			  interval)
		{
		}


		public AggregateException AggregateExceptionToSend { get; set; }

		public DateTime? ErrorOccured { get; set; }

		protected override Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                           CancellationToken cancellationToken)
		{
			try
			{
				var httpSender = new HttpSender();

				var payload = new JobFailedEntity
				{
					JobId = jobQueueItemEntity.JobId,
					AggregateException = AggregateExceptionToSend,
					Created = ErrorOccured
				};

				var response =
					httpSender.PostAsync(CallbackTemplateUri,
					                     payload,
					                     cancellationToken);
				return response;
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber("Error in TrySendStatus.",
				                                 exp);
				throw;
			}
		}

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			base.Dispose(disposing);

			Logger.DebugWithLineNumber("Finished disposing.");
		}
	}
}