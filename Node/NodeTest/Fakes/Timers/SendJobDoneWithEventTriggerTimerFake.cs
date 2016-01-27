using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
    public class SendJobDoneWithEventTriggerTimerFake : TrySendStatusToManagerTimer
    {
        public SendJobDoneWithEventTriggerTimerFake() : base(null,
            null,
            null)
        {
        }
    }
}