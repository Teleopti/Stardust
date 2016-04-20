using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[UseOnToggle(Toggles.SettingsForPersonPeriodChanged_ToHangfire_38207)]
	public class UpdateFindPersonDataHandlerHangfire : IHandleEvent<SettingsForPersonPeriodChangedEvent>, IRunOnHangfire
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

		public UpdateFindPersonDataHandlerHangfire(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
			_personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}
		
		[UnitOfWork]
		public virtual void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_personFinderReadOnlyRepository.UpdateFindPersonData(@event.IdCollection);
		}
	}

	[UseNotOnToggle(Toggles.SettingsForPersonPeriodChanged_ToHangfire_38207)]
#pragma warning disable 618
	public class UpdateFindPersonDataHandlerServiceBus : IHandleEvent<SettingsForPersonPeriodChangedEvent>, IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

		public UpdateFindPersonDataHandlerServiceBus(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
			_personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}
		
		public void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_personFinderReadOnlyRepository.UpdateFindPersonData(@event.IdCollection);
		}
	}
}
