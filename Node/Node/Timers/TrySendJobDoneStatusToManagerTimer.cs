using System;
using System.Security.Cryptography.X509Certificates;
using Stardust.Node.Extensions;
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

		//public override void Setup(NodeConfiguration nodeConfiguration, Uri getManagerJobDoneTemplateUri)
		//{
		//	CallbackTemplateUri = nodeConfiguration.GetManagerJobDoneTemplateUri();
		//	base.Setup(nodeConfiguration);
		//}
	}
}