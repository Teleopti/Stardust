using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class UpdateGroupingReadModelConsumer : IHandleEvent<PersonCollectionChangedEvent >
	{
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public UpdateGroupingReadModelConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			_groupingReadOnlyRepository.UpdateGroupingReadModel(@event.PersonIdCollection);
		}
	}
}
