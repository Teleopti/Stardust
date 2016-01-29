using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobDoneWithEventTriggerTimerFake : TrySendStatusToManagerTimer
    {
        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        private static readonly ILog Logger = LogManager.GetLogger(typeof (SendJobDoneWithEventTriggerTimerFake));

        public SendJobDoneWithEventTriggerTimerFake(INodeConfiguration nodeConfiguration,
                                                    Uri callbackTemplateUri) : base(nodeConfiguration,
                                                                                    callbackTemplateUri)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            Logger.Info("Send job done with event trigger fake.");

            InvokeTriggerTrySendStatusSucceded();

            Wait.Set();

            return null;
        }
    }
}