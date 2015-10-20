using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public class MailboxSubscriber : IScheduleMessageSubscriber
	{
		public void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage)
		{
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				scenario,
				typeof(Scenario),
				typeof(IScheduleChangedMessage),
				period.StartDateTime,
				period.EndDateTime,
				false,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersistableScheduleData),
				period.StartDateTime,
				period.EndDateTime,
				true,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IMeeting),
				true,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersonRequest),
				true,
				true);
		}

		public void Unsubscribe(EventHandler<EventMessageArgs> onEventMessage)
		{
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.UnregisterSubscription(onEventMessage);
		}
	}
}