using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class UpdateGroupingReadModelConsumer : IHandleEvent<PersonCollectionChangedEvent >
	{
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public UpdateGroupingReadModelConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		    _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_groupingReadOnlyRepository.UpdateGroupingReadModel(@event.PersonIdCollection);
				uow.PersistAll();
			}
		}
	}
}
