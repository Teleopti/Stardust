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
    public class SendJobCanceledTimerFake : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SendJobCanceledTimerFake));

        public int NumberOfTimeCalled;

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public SendJobCanceledTimerFake(INodeConfiguration nodeConfiguration,
                                        Uri callbackTemplateUri,
                                        double interval = 1000) : base(nodeConfiguration,
                                                                       callbackTemplateUri,
                                                                       interval)
        {
        }

        protected override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo,
                                                                   CancellationToken cancellationToken)
        {
            NumberOfTimeCalled++;

            Wait.Set();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var request = new HttpRequestMessage(HttpMethod.Post,
                                                 "JobCanceled");
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}