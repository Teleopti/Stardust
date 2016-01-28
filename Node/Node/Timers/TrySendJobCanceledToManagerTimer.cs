using Stardust.Node.Constants;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
    {
        public TrySendJobCanceledToManagerTimer(JobToDo jobToDo,
                                                INodeConfiguration nodeConfiguration,
                                                double interval = 10000) : base(jobToDo,
                                                                                nodeConfiguration,
                                                                                nodeConfiguration.GetManagerJobHasBeenCanceledUri(),
                                                                                null,
                                                                                interval)
        {
        }
    }
}