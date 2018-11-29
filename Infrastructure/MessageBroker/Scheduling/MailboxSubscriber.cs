using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public class MailboxSubscriber : IScheduleMessageSubscriber
	{
		public void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage)
		{
			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				scenario,
				typeof(Scenario),
				typeof(IScheduleChangedMessage),
				period.StartDateTime,
				period.EndDateTime,
				false,
				true);

			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersistableScheduleData),
				period.StartDateTime,
				period.EndDateTime,
				true,
				true);

			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IMeeting),
				true,
				true);

			MessageBrokerInStateHolder.Instance.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersonRequest),
				true,
				true);
		}

		public void Unsubscribe(EventHandler<EventMessageArgs> onEventMessage)
		{
			MessageBrokerInStateHolder.Instance.UnregisterSubscription(onEventMessage);
		}
	}
}