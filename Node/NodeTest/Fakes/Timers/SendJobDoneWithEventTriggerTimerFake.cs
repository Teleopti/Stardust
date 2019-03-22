using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Fakes.Timers
{
	public class SendJobDoneWithEventTriggerTimerFake : TrySendStatusToManagerTimer
	{
		public ManualResetEventSlim Wait = new ManualResetEventSlim();

		public SendJobDoneWithEventTriggerTimerFake(NodeConfiguration nodeConfiguration,
		                                            Uri callbackTemplateUri,
		                                            JobDetailSender jobDetailSender,
		                                            IHttpSender httpSender) : base(jobDetailSender,
		                                                                           httpSender)
		{
		}

		protected override Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                           CancellationToken cancellationToken)
		{
			InvokeTriggerTrySendStatusSucceded();

			Wait.Set();

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var request = new HttpRequestMessage(HttpMethod.Post,
			                                     "JobDoneTrigger");
			response.RequestMessage = request;
			return Task.FromResult(response);
		}
	}
}