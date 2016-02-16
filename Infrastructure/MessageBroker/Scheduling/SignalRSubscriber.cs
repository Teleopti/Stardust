using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public class SignalRSubscriber : IScheduleMessageSubscriber
	{
		public void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage)
		{
			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				scenario,
				typeof(Scenario),
				typeof(IScheduleChangedEvent),
				period.StartDateTime,
				period.EndDateTime);

			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersistableScheduleData),
				period.StartDateTime,
				period.EndDateTime);
			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IMeeting));
			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersonRequest));
		}

		public void Unsubscribe(EventHandler<EventMessageArgs> onEventMessage)
		{
			MessageBrokerInStateHolder.Instance.UnregisterSubscription(onEventMessage);
		}
	}
}