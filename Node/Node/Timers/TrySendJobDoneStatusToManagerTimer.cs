using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobDoneStatusToManagerTimer : TrySendStatusToManagerTimer
    {
        public TrySendJobDoneStatusToManagerTimer(INodeConfiguration nodeConfiguration,
                                                  double interval = 10000) : base(nodeConfiguration,
                                                                                  nodeConfiguration.GetManagerJobDoneTemplateUri(),
                                                                                  interval)
        {
        }
    }
}