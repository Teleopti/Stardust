using System;
using System.Collections.Generic;
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
			var detailModel = new AdherenceDetailModel
			{
				Name = @event.Name,
				StartTime = @event.StartTime,
				IsInAdherence = @event.InAdherence,
			};
			var readModel = _persister.Get(@event.PersonId, new DateOnly(@event.StartTime));
			if (readModel == null)
			{
				var model = new AdherenceDetailsReadModel
				{
					PersonId = @event.PersonId,
					Date = new DateOnly(@event.StartTime),
					Model = new AdherenceDetailsModel
					{
						DetailModels = new List<AdherenceDetailModel>()
					}
				};
				model.Model.DetailModels.Add(detailModel);
				_persister.Add(model);
				return;
			}

			var previous = readModel.Model.DetailModels.LastOrDefault();
			if (previous != null)
			{
				detailModel.ActualStartTime = calculateActualStartTime(previous, @event);

				if (noActivityStarted(previous))
					_persister.ClearDetails(readModel);
				else
				{
					updateAdherence(previous, @event.StartTime);
					readModel.Model.HasActivityEnded = true;
				}
			}
			readModel.Model.DetailModels.Add(detailModel);
			readModel.Model.ActualEndTime = null;
			_persister.Update(readModel);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			var readModel = _persister.Get(@event.PersonId, new DateOnly(@event.Timestamp));
			if (readModel == null)
			{
				var model = new AdherenceDetailsReadModel
				{
					PersonId = @event.PersonId,
					Date = new DateOnly(@event.Timestamp),
					Model = new AdherenceDetailsModel
					{
						DetailModels = new List<AdherenceDetailModel>
						{
							new AdherenceDetailModel
							{
								LastStateChangedTime = @event.Timestamp
							}
						}
					}
				};
				_persister.Add(model);
				return;
			}

			var existingModel = readModel.Model.DetailModels.LastOrDefault();
			if (existingModel == null)
			{
				var detailModel = new AdherenceDetailModel
				{
					LastStateChangedTime = @event.Timestamp
				};
				readModel.Model.DetailModels.Add(detailModel);
			}
			else
			{
				if (readModel.Model.HasActivityEnded)
				{
					readModel.Model.ActualEndTime = calculateActualEndTimeWhenActivityEnds(readModel, @event);
				}
				else
				{
					updateAdherence(existingModel, @event.Timestamp);

					if (lateForActivity(existingModel, @event))
						existingModel.ActualStartTime = @event.Timestamp;

					existingModel.LastStateChangedTime = @event.Timestamp;
					existingModel.IsInAdherence = @event.InAdherence;
					readModel.Model.ActualEndTime = calculateActualEndTimeBeforeActivityEnds(readModel, @event);
				}
				_persister.Update(readModel);
			}
		}

		private static DateTime? calculateActualEndTimeWhenActivityEnds(AdherenceDetailsReadModel model,
			PersonStateChangedEvent @event)
		{
			if (!@event.InAdherenceWithPreviousActivity && model.Model.ActualEndTime == null)
			{
				return @event.Timestamp;
			}
			return model.Model.ActualEndTime;
		}

		private static DateTime? calculateActualEndTimeBeforeActivityEnds(AdherenceDetailsReadModel model,
			PersonStateChangedEvent @event)
		{
			if (@event.InAdherence)
				return null;
			if (!@event.InAdherence && model.Model.ActualEndTime == null)
				return @event.Timestamp;
			return model.Model.ActualEndTime;
		}



		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			var readModel = _persister.Get(@event.PersonId, new DateOnly(@event.ShiftStartTime));
			if (readModel == null)
				return;
			var lastModel = readModel.Model.DetailModels.LastOrDefault();
			if (lastModel == null)
				return;
			
			updateAdherence(lastModel, @event.ShiftEndTime);
			readModel.Model.HasActivityEnded = true;
			readModel.Model.ShiftEndTime = @event.ShiftEndTime;
			_persister.Update(readModel);
		}

		private static bool lateForActivity(AdherenceDetailModel model, PersonStateChangedEvent @event)
		{
			return (@event.InAdherence && model.ActualStartTime == null);
		}

		private static DateTime? calculateActualStartTime(AdherenceDetailModel model, PersonActivityStartEvent @event)
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

		private static bool noActivityStarted(AdherenceDetailModel model)
		{
			return model.Name == null;
		}

		private static void updateAdherence(AdherenceDetailModel model, DateTime timestamp)
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