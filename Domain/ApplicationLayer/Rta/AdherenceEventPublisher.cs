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

		public void Publish(StateInfo info, DateTime time, Adherence toAdherence)
		{
			if (toAdherence == Adherence.In)
				_eventPublisher.Publish(new PersonInAdherenceEvent
				{
					ScheduleDate = info.IsScheduled ? new DateOnly(info.CurrentShiftStartTime): new DateOnly(time),
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
					ShiftEndTime = info.IsScheduled ? info.CurrentShiftEndTime : new DateOnly(time)
				});

			if (toAdherence == Adherence.Out)
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent
				{
					ScheduleDate = info.IsScheduled ? new DateOnly(info.CurrentShiftStartTime) : new DateOnly(time),
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
					ShiftEndTime = info.IsScheduled ? info.CurrentShiftEndTime : new DateOnly(time)
				});

		}

	}
}