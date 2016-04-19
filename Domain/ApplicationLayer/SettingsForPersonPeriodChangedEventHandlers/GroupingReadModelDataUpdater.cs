using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[UseOnToggle(Toggles.SettingsForPersonPeriodChanged_ToHangfire_38207)]
	public class GroupingReadModelDataUpdaterHangfire : 
		IHandleEvent<SettingsForPersonPeriodChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelDataUpdaterHangfire(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		[UnitOfWork]
		public virtual void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelData(@event.IdCollection);
		}
	}

	[UseNotOnToggle(Toggles.SettingsForPersonPeriodChanged_ToHangfire_38207)]
	public class GroupingReadModelDataUpdaterServiceBus :
		IHandleEvent<SettingsForPersonPeriodChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelDataUpdaterServiceBus(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelData(@event.IdCollection);
		}
	}
}