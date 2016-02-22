using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TrySendJobCanceledToManagerTimer));

        public TrySendJobCanceledToManagerTimer(INodeConfiguration nodeConfiguration,
                                                double interval = 10000) : base(nodeConfiguration,
                                                                                nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri(),
                                                                                interval)
        {
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.LogDebugWithLineNumber(Logger,"Start disposing.");

            base.Dispose(disposing);

            LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
        }
    }
}