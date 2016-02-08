using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobDoneWithEventTriggerTimerFake : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SendJobDoneWithEventTriggerTimerFake));

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public SendJobDoneWithEventTriggerTimerFake(INodeConfiguration nodeConfiguration,
                                                    Uri callbackTemplateUri) : base(nodeConfiguration,
                                                                                    callbackTemplateUri)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            InvokeTriggerTrySendStatusSucceded();

            Wait.Set();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var request = new HttpRequestMessage(HttpMethod.Post, CallbackTemplateUri);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}