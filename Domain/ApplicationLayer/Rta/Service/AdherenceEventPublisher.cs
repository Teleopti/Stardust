using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceEventPublisher : IAdherenceEventPublisher
	{
		private readonly IRtaDecoratingEventPublisher _eventPublisher;

		public AdherenceEventPublisher(IRtaDecoratingEventPublisher	eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info, DateTime time, EventAdherence toAdherence)
		{
			if (toAdherence == EventAdherence.In)
			{
				_eventPublisher.Publish(info, new PersonInAdherenceEvent
				{
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId
				});
			}

			if (toAdherence == EventAdherence.Out)
			{
				_eventPublisher.Publish(info, new PersonOutOfAdherenceEvent
				{
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId
				});
			}

			if (toAdherence == EventAdherence.Neutral)
			{
				_eventPublisher.Publish(info, new PersonNeutralAdherenceEvent
				{
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