using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net;
using Stardust.Node.Log4Net.Extensions;

namespace Stardust.Node.Timers
{
	public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobCanceledToManagerTimer));

		public TrySendJobCanceledToManagerTimer(INodeConfiguration nodeConfiguration,
												TrySendJobProgressToManagerTimer sendJobProgressToManagerTimer,
												double interval = 500) : base(nodeConfiguration,
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