using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class UpdateFindPersonConsumer : IHandleEvent<PersonCollectionChangedEvent>
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public UpdateFindPersonConsumer(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
        }

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			_personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection);
		}
	}
}
