using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Core.Node.Timers;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
	{
		private readonly IHttpSender _httpSender;

		public TrySendJobFaultedToManagerTimer(JobDetailSender jobDetailSender,
		                                       IHttpSender httpSender,
		                                       double interval = 500) : base(jobDetailSender,
		                                                                     httpSender,
		                                                                     interval)
		{
			_httpSender = httpSender;
		}

		//public override void Setup(NodeConfiguration nodeConfiguration, Uri getManagerJobDoneTemplateUri)
		//{
		//	CallbackTemplateUri = nodeConfiguration.GetManagerJobHasFailedTemplatedUri();
		//	base.Setup(nodeConfiguration);
		//}


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

			catch (Exception exception)
			{
				_exceptionLoggerHandler.LogError("Error in TrySendJobFaultedTimer.", exception);
				throw;
			}
		}
	}
}