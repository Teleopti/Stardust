using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
    {
        public TrySendJobFaultedToManagerTimer(JobToDo jobToDo,
                                               INodeConfiguration nodeConfiguration,
                                               double interval = 10000) : base(jobToDo,
                                                                               nodeConfiguration,
                                                                               nodeConfiguration.GetManagerJobHasFailedUri(),
                                                                               null,
                                                                               interval)
        {
        }
    }
}