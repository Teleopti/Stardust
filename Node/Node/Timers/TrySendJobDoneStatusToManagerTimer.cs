using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobDoneStatusToManagerTimer : TrySendStatusToManagerTimer
	{
		public TrySendJobDoneStatusToManagerTimer(JobDetailSender jobDetailSender,
												  IHttpSender httpSender,
												  double interval = 500) : base(jobDetailSender,
																				  httpSender,
																				  interval)
		{
		}
	}
}