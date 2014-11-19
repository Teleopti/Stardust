using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceDetailsReadModelUpdater :
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly IAdherenceDetailsReadModelPersister _persister;

		public AdherenceDetailsReadModelUpdater(IAdherenceDetailsReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Handle(PersonActivityStartEvent @event)
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
				model.ActualStartTime = calculateActualStartTime(previous, @event.InAdherence, @event.StartTime);

				if (noActivityStarted(previous))
					_persister.Remove(model.PersonId, model.BelongsToDate);
				else
				{
					updateAdherence(previous, @event.StartTime);
					_persister.Update(previous);
				}
			}

			_persister.Add(model);	
		}

		private static DateTime? calculateActualStartTime(AdherenceDetailsReadModel model, bool inAdherence, DateTime timestamp)
		{
			if (inAdherence && model.IsInAdherence)
			{
				return timestamp;
			}
			if (inAdherence && !model.IsInAdherence)
			{
				return model.LastStateChangedTime;
			}
			return null;
		}

		private static bool noActivityStarted(AdherenceDetailsReadModel model)
		{
			return model.Name == null;
		}

		public void Handle(PersonStateChangedEvent @event)
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

				if (@event.InAdherence && existingModel.ActualStartTime == null)
					existingModel.ActualStartTime = @event.Timestamp;
				existingModel.LastStateChangedTime = @event.Timestamp;
				existingModel.IsInAdherence = @event.InAdherence;
				_persister.Update(existingModel);
			}
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			
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