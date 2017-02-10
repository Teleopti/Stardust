using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
	public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
	{
		public TrySendJobCanceledToManagerTimer(NodeConfiguration nodeConfiguration,
												TrySendJobDetailToManagerTimer sendJobDetailToManagerTimer,
												IHttpSender httpSender,
												double interval = 500) : base(nodeConfiguration,
		                                                                        nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri(),
																				sendJobDetailToManagerTimer,
																				httpSender,
																				interval)
		{
		}
	}
}