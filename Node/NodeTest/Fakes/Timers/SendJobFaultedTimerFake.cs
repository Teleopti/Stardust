using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobFaultedTimerFake : TrySendStatusToManagerTimer
    {
        public int NumberOfTimeCalled;

        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        public SendJobFaultedTimerFake(INodeConfiguration nodeConfiguration = null,
                                       Uri callbackTemplateUri = null,
                                       double interval = 10000) : base(nodeConfiguration,
                                                                       callbackTemplateUri,
                                                                       interval)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            Wait.Set();

            NumberOfTimeCalled ++;

            return null;
        }
    }
}