using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
#pragma warning disable 618
	public class ScheduleChangedNotifier : 
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IMessageCreator _broker;

		public ScheduleChangedNotifier(IMessageCreator broker)
		{
			_broker = broker;
		}

		public void Handle(ScheduleChangedEvent @event)
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