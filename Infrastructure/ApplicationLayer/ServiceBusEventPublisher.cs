using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IEventPublisher, ICurrentEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(ResolveEventHandlers resolver, IServiceBusSender sender)
		{
			_resolver = resolver;
			_sender = sender;
		}

		public void Publish(params IEvent[] events)
		{
			events.Where(e => _resolver.ResolveServiceBusHandlersForEvent(e).Any())
				.Batch(100)
				.ForEach(b =>
					_sender.Send(true, b.ToArray())
				);
		}

		public IEventPublisher Current()
		{
			return this;
		}
	}
}