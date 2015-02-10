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
		private readonly AdherencePercentageCalculator _calculator;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister, AdherencePercentageCalculator calculator)
		{
			_persister = persister;
			_calculator = calculator;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				new AdherencePercentageState
				{
					Timestamp = @event.Timestamp, 
					Adherence = Adherence.In
				}, 
				m => m.IsLastTimeInAdherence = true);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId, 
				new AdherencePercentageState
				{
					Timestamp = @event.Timestamp, 
					Adherence = Adherence.Out
				}, 
				m => m.IsLastTimeInAdherence = false);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handleEvent(
				@event.PersonId,
				new AdherencePercentageState
				{
					Timestamp = @event.ShiftEndTime,
					Adherence = Adherence.None,
					ShiftEnded = true
				},
				m => m.ShiftHasEnded = true);
		}

		private void handleEvent(Guid personId, AdherencePercentageState state, Action<AdherencePercentageReadModel> mutate)
		{
			var model = _persister.Get(new DateOnly(state.Timestamp), personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = new DateOnly(state.Timestamp),
					PersonId = personId,
					LastTimestamp = state.Timestamp,
				};
				model.Saga.Add(state);
			}
			else
			{
				model.Saga.Add(state);
				_calculator.Calculate(model);
			}
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