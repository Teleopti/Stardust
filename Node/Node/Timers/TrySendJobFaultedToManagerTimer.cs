using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
    {
        public TrySendJobFaultedToManagerTimer(INodeConfiguration nodeConfiguration,
                                               double interval = 10000) : base(nodeConfiguration,
                                                                               nodeConfiguration.GetManagerJobHasFaileTemplatedUri(),
                                                                               interval)
        {
        }
    }
}