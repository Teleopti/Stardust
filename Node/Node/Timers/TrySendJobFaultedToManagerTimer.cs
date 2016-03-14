using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
	public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobFaultedToManagerTimer));

		public TrySendJobFaultedToManagerTimer(INodeConfiguration nodeConfiguration,
											   TrySendJobProgressToManagerTimer sendJobProgressToManagerTimer,
											   double interval = 2000) : base(nodeConfiguration,
		                                                                       nodeConfiguration.GetManagerJobHasFaileTemplatedUri(),
																			   sendJobProgressToManagerTimer,
																			   interval)
		{
		}

		protected override void Dispose(bool disposing)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start disposing.");

			base.Dispose(disposing);

			LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
		}
	}
}