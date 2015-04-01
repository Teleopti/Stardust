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

		public void Publish(StateInfo info, DateTime time, AdherenceState toAdherence)
		{
			if (toAdherence == AdherenceState.In)
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

			if (toAdherence == AdherenceState.Out)
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

			if (toAdherence == AdherenceState.Neutral)
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