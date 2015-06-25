using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public class SignalRSubscriber : IScheduleChangeSubscriber
	{
		public void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage)
		{
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				scenario,
				typeof(Scenario),
				typeof(IScheduleChangedEvent),
				period.StartDateTime,
				period.EndDateTime);
		}
	}
}