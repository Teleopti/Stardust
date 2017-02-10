using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Timers
{
	public class TrySendJobDoneStatusToManagerTimer : TrySendStatusToManagerTimer
	{
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
	}
}