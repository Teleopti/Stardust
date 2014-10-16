using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModelUpdater :
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, true);
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, false);
		}

		private void handleEvent(Guid personId, DateTime timestamp, bool isInAdherence)
		{
			var model = getModel(personId, timestamp, isInAdherence);
			incrementMinutes(model, timestamp);
			model.IsLastTimeInAdherence = isInAdherence;
			_persister.Persist(model);
		}

		private static void incrementMinutes(AdherencePercentageReadModel model, DateTime timestamp)
		{
			if (model.IsLastTimeInAdherence)
				model.MinutesInAdherence += Convert.ToInt32((timestamp - model.LastTimestamp).TotalMinutes);
			else
				model.MinutesOutOfAdherence += Convert.ToInt32((timestamp - model.LastTimestamp).TotalMinutes);
			model.LastTimestamp = timestamp;
		}

		private AdherencePercentageReadModel getModel(Guid personId, DateTime timestamp, bool currentlyInAdherence)
		{
			var model = _persister.Get(new DateOnly(timestamp), personId);
			if (model == null)
				model = makeModel(personId, timestamp, currentlyInAdherence);
			return model;
		}

		private static AdherencePercentageReadModel makeModel(Guid personId, DateTime timestamp, bool currentlyInAdherence)
		{
			return new AdherencePercentageReadModel
			{
				Date = new DateOnly(timestamp),
				PersonId = personId,
				MinutesInAdherence = 0,
				LastTimestamp = timestamp,
				IsLastTimeInAdherence = currentlyInAdherence
			};
		}

	}
}