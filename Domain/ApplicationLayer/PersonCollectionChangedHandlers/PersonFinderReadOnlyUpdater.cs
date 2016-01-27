using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class PersonFinderReadOnlyUpdater : 
		IHandleEvent<PersonCollectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public PersonFinderReadOnlyUpdater(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
        }

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			_personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection);
		}
	}

	public class PersonCollectionChangedEventPublisherOfLegacyPersonChangedMessage : 
		IHandleEvent<PersonChangedMessage>,
		IRunOnServiceBus
	{
		private readonly IEventPublisher _publisher;

		public PersonCollectionChangedEventPublisherOfLegacyPersonChangedMessage(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Handle(PersonChangedMessage @event)
		{
			_publisher.Publish(new PersonCollectionChangedEvent
			{
				BusinessUnitId = @event.BusinessUnitId,
				Datasource = @event.Datasource,
				InitiatorId = @event.InitiatorId,
				SerializedPeople = @event.SerializedPeople,
				Timestamp = @event.Timestamp
			});
		}
	}
}
