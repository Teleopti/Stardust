using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobFaultedToManagerTimer));
		private readonly IHttpSender _httpSender;

		public TrySendJobFaultedToManagerTimer(NodeConfiguration nodeConfiguration,
		                                       TrySendJobDetailToManagerTimer sendJobDetailToManagerTimer,
		                                       IHttpSender httpSender,
		                                       double interval = 500) : base(nodeConfiguration,
		                                                                     nodeConfiguration
			                                                                     .GetManagerJobHasFailedTemplatedUri(),
		                                                                     sendJobDetailToManagerTimer,
		                                                                     httpSender,
		                                                                     interval)
		{
			_httpSender = httpSender;
		}


		public AggregateException AggregateExceptionToSend { get; set; }

		public DateTime? ErrorOccured { get; set; }

		protected override Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                           CancellationToken cancellationToken)
		{
			try
			{
				var payload = new JobFailedEntity
				{
					JobId = jobQueueItemEntity.JobId,
					AggregateException = AggregateExceptionToSend,
					Created = ErrorOccured
				};

				var response = _httpSender.PostAsync(CallbackTemplateUri,
				                                     payload,
				                                     cancellationToken);
				return response;
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber("Error in TrySendJobFaultedTimer.", exp);
				throw;
			}
		}
	}
}