using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
    {        
        public TrySendJobCanceledToManagerTimer(INodeConfiguration nodeConfiguration,
                                                double interval = 10000) : base(nodeConfiguration,
                                                                                nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri(),
                                                                                interval)
        {
        }
    }
}