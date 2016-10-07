using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	public class GroupingReadModelGroupPageUpdater : 
		IHandleEvent<GroupPageCollectionChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelGroupPageUpdater(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}
		
		[UnitOfWork]
		public virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(@event.GroupPageIdCollection);
		}
	}
}