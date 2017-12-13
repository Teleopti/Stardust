using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	public class DefaultQueueEvent : IEvent
	{
	}

	public class CriticalScheduleChangesTodayQueueEvent : IEvent
	{
	}

	public class QueuingHandler :
		IHandleEvent<DefaultQueueEvent>,
		IHandleEventOnQueue<DefaultQueueEvent>,
		IHandleEvent<CriticalScheduleChangesTodayQueueEvent>,
		IHandleEventOnQueue<CriticalScheduleChangesTodayQueueEvent>,
		IRunOnHangfire
	{
		public void Handle(DefaultQueueEvent @event)
		{
		}

		public string QueueTo(DefaultQueueEvent @event)
		{
			return null;
		}

		public void Handle(CriticalScheduleChangesTodayQueueEvent @event)
		{
		}

		public string QueueTo(CriticalScheduleChangesTodayQueueEvent @event)
		{
			return Queues.CriticalScheduleChangesToday;
		}
	}
}