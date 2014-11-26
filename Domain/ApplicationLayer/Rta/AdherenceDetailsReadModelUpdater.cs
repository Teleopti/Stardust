using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[OnlyHandleWhenEnabled(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	public class AdherenceDetailsReadModelUpdater :
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonShiftEndEvent>
	{
		private readonly IAdherenceDetailsReadModelPersister _persister;

		public AdherenceDetailsReadModelUpdater(IAdherenceDetailsReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonActivityStartEvent @event)
		{
			var model = new AdherenceDetailsReadModel
			{
				Name = @event.Name,
				StartTime = @event.StartTime,
				PersonId = @event.PersonId,
				Date = new DateOnly(@event.StartTime),
				IsInAdherence = @event.InAdherence
			};
			var previous = _persister.Get(@event.PersonId, new DateOnly(@event.StartTime)).LastOrDefault();
			if (previous != null)
			{
				model.ActualStartTime = calculateActualStartTime(previous, @event);
				
				if (noActivityStarted(previous))
					_persister.Remove(model.PersonId, model.BelongsToDate);
				else
				{
					updateAdherence(previous, @event.StartTime);
					previous.ActivityHasEnded = true;
					_persister.Update(previous);
				}
			}

			_persister.Add(model);	
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			var existingModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp)).LastOrDefault();
			if (existingModel == null)
			{
				var model = new AdherenceDetailsReadModel
				{
					PersonId = @event.PersonId,
					Date = new DateOnly(@event.Timestamp),
					LastStateChangedTime = @event.Timestamp,
				};
				_persister.Add(model);
			}
			else
			{
				if (existingModel.ActivityHasEnded)
				{
					existingModel.ActualEndTime = calculateActualEndTimeWhenActivityEnds(existingModel, @event);
				}
				else
				{
					updateAdherence(existingModel, @event.Timestamp);

					if (lateForActivity(existingModel, @event))
						existingModel.ActualStartTime = @event.Timestamp;

					existingModel.LastStateChangedTime = @event.Timestamp;
					existingModel.IsInAdherence = @event.InAdherence;
					existingModel.ActualEndTime = calculateActualEndTimeBeforeActivityEnds(existingModel, @event);
				}
				_persister.Update(existingModel);

			}
		}

		private static DateTime? calculateActualEndTimeWhenActivityEnds(AdherenceDetailsReadModel model,
			PersonStateChangedEvent @event)
		{
			if (!@event.InAdherenceWithPreviousActivity && model.ActualEndTime == null)
			{
				return @event.Timestamp;
			}
			return model.ActualEndTime;
		}

		private static DateTime? calculateActualEndTimeBeforeActivityEnds(AdherenceDetailsReadModel model,
			PersonStateChangedEvent @event)
		{
			if (@event.InAdherence)
				return null;
			if (!@event.InAdherence && model.ActualEndTime == null)
				return @event.Timestamp;
			return model.ActualEndTime;
		}



		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			var lastModel = _persister.Get(@event.PersonId, new DateOnly(@event.ShiftStartTime)).LastOrDefault();
			if (lastModel == null) return;
			
			updateAdherence(lastModel, @event.ShiftEndTime);
			lastModel.ActivityHasEnded = true;
			_persister.Update(lastModel);
		}

		private static bool lateForActivity(AdherenceDetailsReadModel model, PersonStateChangedEvent @event)
		{
			return (@event.InAdherence && model.ActualStartTime == null);
		}

		private static DateTime? calculateActualStartTime(AdherenceDetailsReadModel model, PersonActivityStartEvent @event)
		{
			if (@event.InAdherence && model.IsInAdherence)
			{
				return @event.StartTime;
			}
			if (@event.InAdherence && !model.IsInAdherence)
			{
				return model.LastStateChangedTime;
			}
			return null;
		}

		private static bool noActivityStarted(AdherenceDetailsReadModel model)
		{
			return model.Name == null;
		}

		private static void updateAdherence(AdherenceDetailsReadModel model, DateTime timestamp)
		{
			var timeToAdd = model.LastStateChangedTime.HasValue
				? timestamp - model.LastStateChangedTime.Value
				: timestamp - model.StartTime;

			if (model.IsInAdherence)
				model.TimeInAdherence += timeToAdd.Value;
			else
				model.TimeOutOfAdherence += timeToAdd.Value;
		}
	}
}