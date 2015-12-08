using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class AdherencePercentageReadModelUpdater :
		IRunOnHangfire,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<PersonShiftStartEvent>,
		IHandleEvent<PersonShiftEndEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IInitializeble
	{
		private readonly IAdherencePercentageReadModelPersister _persister;

		public AdherencePercentageReadModelUpdater(IAdherencePercentageReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftStartEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.ShiftStartTime,
					ShiftStarted = true
				},
				m => { });
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
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
 				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp, 
					InAdherence = false
				}, 
				m => m.IsLastTimeInAdherence = false);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.Timestamp,
					InAdherence = null
				},
				m => m.IsLastTimeInAdherence = null);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			handleEvent(
				@event.PersonId,
				@event.BelongsToDate,
				new AdherencePercentageReadModelState
				{
					Timestamp = @event.ShiftEndTime,
					ShiftEnded = true
				},
				m => m.ShiftHasEnded = true);
		}

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		private void handleEvent(Guid personId, DateOnly? belongsToDate, AdherencePercentageReadModelState readModelState, Action<AdherencePercentageReadModel> mutate)
		{
			var date = belongsToDate.HasValue ? belongsToDate.Value : new DateOnly(readModelState.Timestamp);
			var model = _persister.Get(date, personId);
			if (model == null)
			{
				model = new AdherencePercentageReadModel
				{
					Date = date.Date,
					PersonId = personId,
					LastTimestamp = readModelState.Timestamp,
					State = new[] {readModelState}
				};
			}
			else
			{
				model.State = model.State.Concat(new[] {readModelState});
				if (readModelState.Timestamp - model.LastTimestamp.GetValueOrDefault() > TimeSpan.Zero)
					model.LastTimestamp = readModelState.Timestamp;
				calculate(model);
			}
			mutate(model);
			_persister.Persist(model);
		}
		
		private static void calculate(AdherencePercentageReadModel model)
		{
			AdherencePercentageReadModelState previous = null;
			resetAdherence(model);
			model.State = model.State.OrderBy(x => x.Timestamp).ToArray();
			foreach (var current in model.State)
			{
				if (previous != null)
				{
					if (current.ShiftStarted.GetValueOrDefault(false))
					{
						resetAdherence(model);
						current.InAdherence = previous.InAdherence;
					}
					else if (previous.InAdherence.HasValue)
					{
						var timeDifferenceBetweenCurrentAndPrevious = current.Timestamp - previous.Timestamp;
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

		private static void resetAdherence(AdherencePercentageReadModel model)
		{
			model.TimeInAdherence = TimeSpan.Zero;
			model.TimeOutOfAdherence = TimeSpan.Zero;
		}
	}
}