using Stardust.Core.Node.Interfaces;
using JobDetailSender = Stardust.Core.Node.Workers.JobDetailSender;

namespace Stardust.Core.Node.Timers
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