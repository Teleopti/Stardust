using System;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class StardustHealthCheckHandler: IHandle<StardustHealthCheckEvent>
	{
		public void Handle(StardustHealthCheckEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			sendProgress("Running job on Node");
			sendProgress("This health check is performed with a dummy data source and a dummy business unit");
		}
	}
}