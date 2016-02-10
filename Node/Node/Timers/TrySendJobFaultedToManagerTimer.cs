using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
    public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TrySendJobFaultedToManagerTimer));

        public TrySendJobFaultedToManagerTimer(INodeConfiguration nodeConfiguration,
                                               double interval = 10000) : base(nodeConfiguration,
                                                                               nodeConfiguration.GetManagerJobHasFaileTemplatedUri(),
                                                                               interval)
        {
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.LogInfoWithLineNumber(Logger, "Start disposing.");

            base.Dispose(disposing);

            LogHelper.LogInfoWithLineNumber(Logger, "Finished disposing.");
        }
    }
}