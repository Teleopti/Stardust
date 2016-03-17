using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
	public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobCanceledToManagerTimer));

		public TrySendJobCanceledToManagerTimer(INodeConfiguration nodeConfiguration,
												TrySendJobProgressToManagerTimer sendJobProgressToManagerTimer,
												double interval = 2000) : base(nodeConfiguration,
		                                                                        nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri(),
																				sendJobProgressToManagerTimer,
																				interval)
		{
		}

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			base.Dispose(disposing);

			Logger.DebugWithLineNumber("Finished disposing.");
		}
	}
}