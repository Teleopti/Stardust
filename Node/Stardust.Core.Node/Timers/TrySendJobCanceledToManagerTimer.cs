using Stardust.Core.Node.Interfaces;
using JobDetailSender = Stardust.Core.Node.Workers.JobDetailSender;

namespace Stardust.Core.Node.Timers
{
	public class TrySendJobCanceledToManagerTimer : TrySendStatusToManagerTimer
	{
		public TrySendJobCanceledToManagerTimer(JobDetailSender jobDetailSender,
												IHttpSender httpSender,
												double interval = 500) : base(jobDetailSender,
																				httpSender,
																				interval)
		{
		}

		//public override void Setup(NodeConfiguration nodeConfiguration, Uri getManagerJobDoneTemplateUri)
		//{
		//	CallbackTemplateUri = nodeConfiguration.GetManagerJobHasBeenCanceledTemplateUri();
		//	base.Setup(nodeConfiguration);
		//}
	}
}