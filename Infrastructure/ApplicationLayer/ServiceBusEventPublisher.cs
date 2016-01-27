using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IEventPublisher, ICurrentEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		public void Publish(params IEvent[] events)
		{
			events.Batch(100).ForEach(b => _sender.Send(true, b.ToArray()));
		}

		public IEventPublisher Current()
		{
			return this;
		}
	}
}