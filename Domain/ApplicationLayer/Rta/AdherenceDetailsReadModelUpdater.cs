using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
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
				updateAdherence(existingModel, @event.Timestamp);

				if (lateForActivity(existingModel, @event))
					existingModel.ActualStartTime = @event.Timestamp;

				existingModel.LastStateChangedTime = @event.Timestamp;
				existingModel.IsInAdherence = @event.InAdherence;
				_persister.Update(existingModel);
			}
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			var lastModel = _persister.Get(@event.PersonId, new DateOnly(@event.ShiftStartTime)).Last();
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