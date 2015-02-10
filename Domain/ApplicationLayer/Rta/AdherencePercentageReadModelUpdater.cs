using System;
using System.Linq;
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
			handleEvent(
				@event.PersonId,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp, 
					InAdherence = true
				}, 
				m => m.IsLastTimeInAdherence = true);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId, 
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp, 
					InAdherence = false
				}, 
				m => m.IsLastTimeInAdherence = false);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handleEvent(
				@event.PersonId,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.ShiftEndTime,
					ShiftEnded = true
				},
				m => m.ShiftHasEnded = true);
		}

		private void handleEvent(Guid personId, AdherencePercentageReadModelState readModelState, Action<AdherencePercentageReadModel> mutate)
		{
			var model = _persister.Get(new DateOnly(readModelState.Timestamp), personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = new DateOnly(readModelState.Timestamp),
					PersonId = personId,
					LastTimestamp = readModelState.Timestamp,
				};
				model.State = new[] { readModelState };
			}
			else
			{
				model.State = model.State.Concat(new[] {readModelState});
				calculate(model);
			}
			mutate(model);
			_persister.Persist(model);
		}
		
		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}

		private static void calculate(AdherencePercentageReadModel model)
		{
			model.TimeInAdherence = TimeSpan.Zero;
			model.TimeOutOfAdherence = TimeSpan.Zero;
			AdherencePercentageReadModelState previous = null;
			model.State = model.State.OrderBy(x => x.Timestamp).ToArray();
			foreach (var current in model.State)
			{
				if (previous != null)
				{
					var timeDifferenceBetweenCurrentAndPrevious = current.Timestamp - previous.Timestamp;
					if (previous.InAdherence.HasValue)
					{
						if (previous.InAdherence.Value)
							model.TimeInAdherence += timeDifferenceBetweenCurrentAndPrevious;
						if (!previous.InAdherence.Value)
							model.TimeOutOfAdherence += timeDifferenceBetweenCurrentAndPrevious;
					}
				}
				if (current.ShiftEnded.GetValueOrDefault(false))
					break;
				previous = current;
			}
		}

	}

}