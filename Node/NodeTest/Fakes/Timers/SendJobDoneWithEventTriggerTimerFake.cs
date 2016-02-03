using System;
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
        public ManualResetEventSlim Wait = new ManualResetEventSlim();

        
        public SendJobDoneWithEventTriggerTimerFake(INodeConfiguration nodeConfiguration,
                                                    Uri callbackTemplateUri) : base(nodeConfiguration,
                                                                                    callbackTemplateUri)
        {
        }

        public override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo)
        {
            LogHelper.LogInfoWithLineNumber("Send job done with event trigger fake.");

            InvokeTriggerTrySendStatusSucceded();

            Wait.Set();

            return null;
        }
    }
}