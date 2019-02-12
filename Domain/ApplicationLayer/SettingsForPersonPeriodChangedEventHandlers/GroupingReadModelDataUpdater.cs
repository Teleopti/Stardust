using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	public class GroupingReadModelDataUpdater : 
		IHandleEvent<SettingsForPersonPeriodChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupingReadModelDataUpdater(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		[UnitOfWork]
		public virtual void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModelData(@event.IdCollection.ToArray());
		}
	}
}