using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public void Publish(object @event)
		{
			WTFDEBUG.Log("ServiceBusEventPublisher " + @event.GetType().Name);
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");
			WTFDEBUG.Log("ServiceBusEventPublisher Send " + @event.GetType().Name);
			_sender.Send(@event);
			WTFDEBUG.Log("/ServiceBusEventPublisher " + @event.GetType().Name);
		}
	}
}