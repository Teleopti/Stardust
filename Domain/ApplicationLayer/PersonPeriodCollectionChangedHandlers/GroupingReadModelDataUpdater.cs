using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonPeriodCollectionChangedHandlers
{
	public class GroupingReadModelDataUpdater : 
		IHandleEvent<PersonPeriodCollectionChangedEvent>, 
		IRunOnServiceBus
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelDataUpdater(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public void Handle(PersonPeriodCollectionChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelData(@event.PersonIdCollection);
		}
	}
}