using System;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
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