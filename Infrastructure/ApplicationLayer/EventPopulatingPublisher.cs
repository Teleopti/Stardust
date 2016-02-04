using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class EventPopulatingPublisher : IEventPopulatingPublisher
	{
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

		public EventPopulatingPublisher(ICurrentEventPublisher eventPublisher, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
		{
			_eventPublisher = eventPublisher;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
		}

		public void Publish(params IEvent[] events)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(events);
			_eventPublisher.Current().Publish(events);
		}
	}
}