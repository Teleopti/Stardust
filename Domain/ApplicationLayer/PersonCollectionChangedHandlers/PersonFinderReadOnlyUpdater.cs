using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

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
	
}
