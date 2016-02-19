using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class NodeStartupNotificationToManagerFake : TrySendNodeStartUpNotificationToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeStartupNotificationToManagerFake));

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public NodeStartupNotificationToManagerFake(INodeConfiguration nodeConfiguration,
                                                    Uri callbackTemplateUri,
                                                    double interval = 1000,
                                                    bool autoReset = false) : base(nodeConfiguration,
                                                                                   callbackTemplateUri,
                                                                                   interval,
                                                                                   autoReset)
        {
        }

          
        public override Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress,
                                                                              CancellationToken cancellationToken)
        {
            Wait.Set();

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var request = new HttpRequestMessage(HttpMethod.Post,
                                                 CallbackTemplateUri);

            response.RequestMessage = request;

            return Task.FromResult(response);
        }
    }
}