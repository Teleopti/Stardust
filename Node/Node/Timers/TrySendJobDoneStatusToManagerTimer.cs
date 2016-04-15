using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobDoneStatusToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobDoneStatusToManagerTimer));

		public TrySendJobDoneStatusToManagerTimer(NodeConfiguration nodeConfiguration,
												  TrySendJobDetailToManagerTimer sendJobDetailToManagerTimer,
												  IHttpSender httpSender,
												  double interval = 500) : base(nodeConfiguration,
		                                                                          nodeConfiguration.GetManagerJobDoneTemplateUri(),
																				  sendJobDetailToManagerTimer,
																				  httpSender,
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