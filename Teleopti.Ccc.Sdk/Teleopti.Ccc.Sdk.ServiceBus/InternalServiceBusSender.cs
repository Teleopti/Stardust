using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class InternalServiceBusSender : IServiceBusSender
	{
		private readonly Func<IServiceBus> _serviceBus;

		public InternalServiceBusSender(Func<IServiceBus> serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Dispose()
		{
		}

		public void Send(bool throwOnNoBus, params object[] message)
		{
			_serviceBus().Send(message);
		}

		public bool EnsureBus()
		{
			return true;
		}
	}
}
