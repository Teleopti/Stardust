using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceEventPublisher : IAdherenceEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public AdherenceEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(StateInfo info, DateTime time, Adherence fromAdherence, Adherence toAdherence)
		{
			if (toAdherence == fromAdherence) return;

			if (toAdherence == Adherence.In)
				_eventPublisher.Publish(new PersonInAdherenceEvent
				{
					ScheduleDate = new DateOnly(info.CurrentShiftStartTime),
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
					ShiftEndTime = info.CurrentShiftEndTime
				});

			if (toAdherence == Adherence.Out)
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent
				{
					ScheduleDate = new DateOnly(info.CurrentShiftStartTime),
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
					ShiftEndTime = info.CurrentShiftEndTime
				});

		}

	}
}