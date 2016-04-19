using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GroupPageCollectionChangedHandlers
{
	[UseOnToggle(Toggles.GroupPageCollection_ToHangfire_38178)]
	public class GroupingReadModelGroupPageUpdaterHangfire : 
		IHandleEvent<GroupPageCollectionChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelGroupPageUpdaterHangfire(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}
		
		[UnitOfWork]
		public virtual void Handle(GroupPageCollectionChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(@event.GroupPageIdCollection);
		}
	}

	[UseNotOnToggle(Toggles.GroupPageCollection_ToHangfire_38178)]
	public class GroupingReadModelGroupPageUpdaterServicebus :
		IHandleEvent<GroupPageCollectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelGroupPageUpdaterServicebus(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}
		
		public void Handle(GroupPageCollectionChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(@event.GroupPageIdCollection);
		}
	}
}