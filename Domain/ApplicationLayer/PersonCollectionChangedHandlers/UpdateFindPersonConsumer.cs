using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class UpdateFindPersonConsumer : IHandleEvent<PersonCollectionChangedEvent>
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        public UpdateFindPersonConsumer(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
        }

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection);
				uow.PersistAll();
			}
		}
	}
}
