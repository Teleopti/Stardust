using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobDoneTimerFake : TrySendStatusToManagerTimer
    {
        public int NumberOfTimeCalled;

        public SendJobDoneTimerFake(JobToDo jobToDo = null,
            INodeConfiguration nodeConfiguration = null,
            Uri callbackUri = null,
            ElapsedEventHandler overrideElapsedEventHandler = null,
            double interval = 10000) : base(jobToDo,
                nodeConfiguration,
                callbackUri,
                overrideElapsedEventHandler,
                interval)
        {
        }

        public override async Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            NumberOfTimeCalled++;

            return await base.TrySendStatus(jobToDo);
        }
    }
}