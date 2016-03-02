using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
	public class TrySendJobDoneStatusToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobDoneStatusToManagerTimer));

		public TrySendJobDoneStatusToManagerTimer(INodeConfiguration nodeConfiguration,
		                                          double interval = 2000) : base(nodeConfiguration,
		                                                                          nodeConfiguration
			                                                                          .GetManagerJobDoneTemplateUri(),
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