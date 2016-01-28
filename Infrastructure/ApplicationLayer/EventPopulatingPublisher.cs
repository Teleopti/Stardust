using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class EventPopulatingPublisher : IEventPopulatingPublisher
	{
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly IEventContextPopulator _eventContextPopulator;

		public EventPopulatingPublisher(ICurrentEventPublisher eventPublisher, IEventContextPopulator eventContextPopulator)
		{
			_eventPublisher = eventPublisher;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(params IEvent[] events)
		{
			_eventContextPopulator.PopulateEventContext(events);
			_eventPublisher.Current().Publish(events);
		}
	}
}