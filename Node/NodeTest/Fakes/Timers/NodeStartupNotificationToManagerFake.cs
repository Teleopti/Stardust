using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Fakes.Timers
{
	public class NodeStartupNotificationToManagerFake : TrySendNodeStartUpNotificationToManagerTimer
	{
		public ManualResetEventSlim Wait = new ManualResetEventSlim();

		public NodeStartupNotificationToManagerFake(NodeConfiguration nodeConfiguration,
													IHttpSender httpSender,
		                                            double interval = 1000,
		                                            bool autoReset = false) : base(httpSender,
																				   interval,
		                                                                           autoReset)
		{
            SetupAndStart(nodeConfiguration);
		}


		public override Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress,
																			  Uri callbackToManagerUri,
		                                                                      CancellationToken cancellationToken)
		{
			Wait.Set();

			var response = new HttpResponseMessage(HttpStatusCode.OK);

			var request = new HttpRequestMessage(HttpMethod.Post,
			                                     CallbackToManagerTemplateUri);

			response.RequestMessage = request;

			return Task.FromResult(response);
		}
	}
}