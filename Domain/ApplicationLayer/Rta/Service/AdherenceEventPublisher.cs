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

		public void Publish(StateInfo info, DateTime time, EventAdherence toAdherence)
		{
			if (toAdherence == EventAdherence.In)
			{
				_eventPublisher.Publish(info, new PersonInAdherenceEvent
				{
					PersonId = info.Person.PersonId,
					Timestamp = time,
					BusinessUnitId = info.Person.BusinessUnitId,
					TeamId = info.Person.TeamId,
					SiteId = info.Person.SiteId
				});
			}

			if (toAdherence == EventAdherence.Out)
			{
				_eventPublisher.Publish(info, new PersonOutOfAdherenceEvent
				{
					PersonId = info.Person.PersonId,
					Timestamp = time,
					BusinessUnitId = info.Person.BusinessUnitId,
					TeamId = info.Person.TeamId,
					SiteId = info.Person.SiteId
				});
			}

			if (toAdherence == EventAdherence.Neutral)
			{
				_eventPublisher.Publish(info, new PersonNeutralAdherenceEvent
				{
					PersonId = info.Person.PersonId,
					Timestamp = time,
					BusinessUnitId = info.Person.BusinessUnitId,
					TeamId = info.Person.TeamId,
					SiteId = info.Person.SiteId
				});
			}
		}
	}
}