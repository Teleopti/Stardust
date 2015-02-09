using System;
using System.Collections.Generic;
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
		private readonly AdherencePercentageCalculator _calculator;
		private readonly IList<AdherenceState> adherenceHistory = new List<AdherenceState>();

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister, AdherencePercentageCalculator calculator)
		{
			_persister = persister;
			_calculator = calculator;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			adherenceHistory.Add(new AdherenceState {Timestamp = @event.Timestamp, Adherence = Adherence.In});
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = true);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			adherenceHistory.Add(new AdherenceState {Timestamp = @event.Timestamp, Adherence = Adherence.Out});
			handleEvent(@event.PersonId, @event.Timestamp, m => m.IsLastTimeInAdherence = false);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			adherenceHistory.Add(new AdherenceState
			{
				Timestamp = @event.ShiftEndTime,
				Adherence = Adherence.None,
				ShiftEnded = true
			});
			handleEvent(@event.PersonId, @event.ShiftEndTime, m =>  m.ShiftHasEnded = true);
		}

		private void handleEvent(Guid personId, DateTime time, Action<AdherencePercentageReadModel> mutate)
		{
			var model = _persister.Get(new DateOnly(time), personId);
			if (model == null)
				model = new AdherencePercentageReadModel
				{
					Date = new DateOnly(time),
					PersonId = personId,
					LastTimestamp = time
				};
			else
				_calculator.Calculate(model, adherenceHistory);
			mutate(model);
			_persister.Persist(model);
		}
		
		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}
	}

}