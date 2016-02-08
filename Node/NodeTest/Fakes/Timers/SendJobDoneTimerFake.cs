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
    public class SendJobDoneTimerFake : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SendJobDoneTimerFake));

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public int NumberOfTimeCalled;

        public SendJobDoneTimerFake(INodeConfiguration nodeConfiguration,
                                    Uri callbackTemplateUri,
                                    double interval = 1000) : base(nodeConfiguration,
                                                                    callbackTemplateUri,
                                                                    interval)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
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