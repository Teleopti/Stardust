using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[EnabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ScheduleChangedNotifierHangfire :
		ScheduleChangedNotifier,
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnHangfire
	{
		public ScheduleChangedNotifierHangfire(IMessageCreator broker) : base(broker)
		{
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			HandleBase(@event);
		}
	}

	[DisabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ScheduleChangedNotifierServiceBus :
		ScheduleChangedNotifier,
		IHandleEvent<ScheduleChangedEvent>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ScheduleChangedNotifierServiceBus(IMessageCreator broker) : base(broker)
		{
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			HandleBase(@event);
		}

		
	}

	public class ScheduleChangedNotifier
	{
		private readonly IMessageCreator _broker;

		public ScheduleChangedNotifier(IMessageCreator broker)
		{
			_broker = broker;
		}

		protected void HandleBase(ScheduleChangedEvent @event)
		{
			if (@event.SkipDelete) return;

			_broker.Send(
				@event.LogOnDatasource,
				@event.LogOnBusinessUnitId,
				@event.StartDateTime,
				@event.EndDateTime,
				@event.InitiatorId,
				@event.ScenarioId,
				typeof (Common.Scenario),
				@event.PersonId,
				typeof (IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null);
		}
	}
}