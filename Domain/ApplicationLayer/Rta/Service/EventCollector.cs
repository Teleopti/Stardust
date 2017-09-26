using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class EventCollector : IEventPublisher
	{
		private readonly IList<IEvent> _events = new List<IEvent>();
		private readonly ICurrentEventPublisher _publisher;

		public EventCollector(ICurrentEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(_events.Add);
		}

		public void Publish()
		{
			var publisher = _publisher.Current();
			var toPublish = _events
				.OrderBy(@event =>
				{
					if (@event is PersonShiftStartEvent)
						return (@event as PersonShiftStartEvent).ShiftStartTime;
					if (@event is PersonShiftEndEvent)
						return (@event as PersonShiftEndEvent).ShiftEndTime;
					if (@event is PersonActivityStartEvent)
						return (@event as PersonActivityStartEvent).StartTime;
					if (@event is PersonStateChangedEvent)
						return (@event as PersonStateChangedEvent).Timestamp;
					if (@event is PersonOutOfAdherenceEvent)
						return (@event as PersonOutOfAdherenceEvent).Timestamp;
					if (@event is PersonInAdherenceEvent)
						return (@event as PersonInAdherenceEvent).Timestamp;
					if (@event is PersonNeutralAdherenceEvent)
						return (@event as PersonNeutralAdherenceEvent).Timestamp;

					return DateTime.MinValue;
				})
				.ToArray();
			publisher.Publish(toPublish);
		}
	}
}