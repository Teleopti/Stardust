using System;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class StardustHealthCheckHandler: IHandle<StardustHealthCheckEvent>
	{
		[AsSystem]
		public void Handle(StardustHealthCheckEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			sendProgress("Running job on Node");
		}
	}
}