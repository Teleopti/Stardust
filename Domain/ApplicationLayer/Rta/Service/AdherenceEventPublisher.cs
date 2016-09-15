using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public AdherenceEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info, DateTime time, EventAdherence toAdherence)
		{
			if (toAdherence == EventAdherence.In)
			{
				_eventPublisher.Publish(new PersonInAdherenceEvent
				{
					BelongsToDate = info.Schedule.BelongsToDate,
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId
				});
			}

			if (toAdherence == EventAdherence.Out)
			{
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent
				{
					BelongsToDate = info.Schedule.BelongsToDate,
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId
				});
			}

			if (toAdherence == EventAdherence.Neutral)
			{
				_eventPublisher.Publish(new PersonNeutralAdherenceEvent
				{
					BelongsToDate = info.Schedule.BelongsToDate,
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId
				});
			}
		}
	}
}