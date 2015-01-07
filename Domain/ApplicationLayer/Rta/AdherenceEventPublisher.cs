using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

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
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
				});

			if (toAdherence == Adherence.Out)
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent
				{
					PersonId = info.PersonId,
					Timestamp = time,
					BusinessUnitId = info.BusinessUnitId,
					TeamId = info.TeamId,
					SiteId = info.SiteId,
				});

		}

	}
}