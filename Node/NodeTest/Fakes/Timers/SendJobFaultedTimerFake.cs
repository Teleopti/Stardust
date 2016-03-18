using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Entities;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
	public class SendJobFaultedTimerFake : TrySendStatusToManagerTimer
	{
		public int NumberOfTimeCalled;

		public ManualResetEventSlim Wait = new ManualResetEventSlim();

		public SendJobFaultedTimerFake(INodeConfiguration nodeConfiguration,
		                               Uri callbackTemplateUri,
									   TrySendJobProgressToManagerTimer sendJobProgressToManagerTimer,
									   IHttpSender httpSender,
									   double interval = 1000) : base(nodeConfiguration,
		                                                              callbackTemplateUri,
																	  sendJobProgressToManagerTimer,
																	  httpSender,
																	  interval)
		{
		}

		protected override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo,
		                                                           CancellationToken cancellationToken)
		{
			NumberOfTimeCalled++;

			Wait.Set();

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var request = new HttpRequestMessage(HttpMethod.Post, "Faulted");
			response.RequestMessage = request;
			return Task.FromResult(response);
		}
	}
}