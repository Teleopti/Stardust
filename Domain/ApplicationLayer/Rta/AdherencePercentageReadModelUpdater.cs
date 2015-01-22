using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModelUpdater :
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonShiftEndEvent>,
		IInitializeble
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = true);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = false);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handleEvent(@event.PersonId, @event.ShiftEndTime, m =>
			{
				m.ShiftHasEnded = true;
				m.LastTimestamp = null;
				m.IsLastTimeInAdherence = null;
			});
		}

		private void handleEvent(Guid personId, DateTime time, Action<AdherencePercentageReadModel> mutate)
		{
			var model = _persister.Get(new DateOnly(time), personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = new DateOnly(time),
					PersonId = personId,
					LastTimestamp = time
				};
			}
			else
			{
				if (model.ShiftHasEnded)
					return;
				incrementTime(model, time);
			}
			mutate(model);
			_persister.Persist(model);
		}

		private static void incrementTime(AdherencePercentageReadModel model, DateTime time)
		{
			if (model.IsLastTimeInAdherence.Value)
				model.TimeInAdherence += (time - model.LastTimestamp.Value);
			else
				model.TimeOutOfAdherence += time - model.LastTimestamp.Value;
			model.LastTimestamp = time;
		}

		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}
	}
}