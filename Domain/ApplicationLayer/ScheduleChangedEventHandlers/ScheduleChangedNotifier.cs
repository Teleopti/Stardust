using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedNotifier : IHandleEvent<ScheduleChangedEvent>
	{
		private readonly IMessageBrokerSender _broker;

		public ScheduleChangedNotifier(IMessageBrokerSender broker)
		{
			_broker = broker;
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			if (@event.SkipDelete) return;

			_broker.Send(
				@event.Datasource,
				@event.BusinessUnitId,
				@event.StartDateTime,
				@event.EndDateTime,
				@event.InitiatorId,
				@event.ScenarioId,
				typeof (Scenario),
				@event.PersonId,
				typeof (IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null);
		}
	}
}