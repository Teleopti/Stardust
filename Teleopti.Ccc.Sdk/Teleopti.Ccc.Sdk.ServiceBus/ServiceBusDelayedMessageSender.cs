using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ServiceBusDelayedMessageSender : IDelayedMessageSender
	{
		private readonly IServiceBus _bus;

		public ServiceBusDelayedMessageSender(IServiceBus bus)
		{
			_bus = bus;
		}
		
		public void DelaySend(DateTime time, object message)
		{
			_bus.DelaySend(time, message);
		}
	}
}