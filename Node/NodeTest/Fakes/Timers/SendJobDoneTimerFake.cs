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
	public class SendJobDoneTimerFake : TrySendJobDoneStatusToManagerTimer
	{
		public int NumberOfTimeCalled;

		public ManualResetEventSlim Wait = new ManualResetEventSlim();

		public SendJobDoneTimerFake(NodeConfiguration nodeConfiguration,
									JobDetailSender jobDetailSender,
									IHttpSender httpSender,
									double interval = 1000) : base(jobDetailSender,
																   httpSender,
																   interval)
		{
		}

		protected override Task<HttpResponseMessage> TrySendStatus(JobQueueItemEntity jobQueueItemEntity,
		                                                           CancellationToken cancellationToken)
		{
			NumberOfTimeCalled++;

			Wait.Set();

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var request = new HttpRequestMessage(HttpMethod.Post, "JobDone");
			response.RequestMessage = request;
			return Task.FromResult(response);
		}
	}
}