using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer
{
	public class EventsConsumer :
		ConsumerOf<IEvent>,
		ConsumerOf<EventsPackageMessage>
	{
		private readonly ServiceBusEventProcessor _processor;
		private readonly IServiceBus _bus;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(EventsConsumer));

		public EventsConsumer(
			ServiceBusEventProcessor processor,
			IServiceBus bus)
		{
			_processor = processor;
			_bus = bus;
		}

		public void Consume(IEvent @event)
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Consuming message of type " + @event.GetType().Name);

			_processor.Process(@event);
		}

		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events.ToArray());
		}
	}
}