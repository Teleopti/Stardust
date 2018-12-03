using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public interface IScheduleMessageSubscriber
	{
		void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage);
		void Unsubscribe(EventHandler<EventMessageArgs> onEventMessage);
	}
}