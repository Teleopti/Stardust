using System;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public interface IScheduleMessageSubscriber
	{
		void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage);
		void Unsubscribe(EventHandler<EventMessageArgs> onEventMessage);
	}
}