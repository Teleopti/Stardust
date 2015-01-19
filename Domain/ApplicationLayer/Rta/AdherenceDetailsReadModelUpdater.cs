using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	public class AdherenceDetailsReadModelUpdater :
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonShiftEndEvent>,
		IInitializeble
	{
		private readonly IAdherenceDetailsReadModelPersister _persister;
		private readonly ILiteTransactionSyncronization _liteTransactionSyncronization;
		private readonly IPerformanceCounter _performanceCounter;

		public AdherenceDetailsReadModelUpdater(IAdherenceDetailsReadModelPersister persister,
			ILiteTransactionSyncronization liteTransactionSyncronization,
			IPerformanceCounter performanceCounter)
		{
			_persister = persister;
			_liteTransactionSyncronization = liteTransactionSyncronization;
			_performanceCounter = performanceCounter;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonActivityStartEvent @event)
		{
			var detailModel = new AdherenceDetailModel
			{
				Name = @event.Name,
				StartTime = @event.StartTime
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
						IsInAdherence = @event.InAdherence,
						Details = new List<AdherenceDetailModel>()
					}
				};
				model.Model.Details.Add(detailModel);
				_persister.Add(model);
				return;
			}

			var previous = readModel.Model.Details.LastOrDefault();
			if (previous != null)
			{
				detailModel.ActualStartTime = calculateActualStartTime(readModel, previous, @event);

				if (shiftHasStarted(previous))
					updateAdherence(readModel, previous, @event.StartTime);
				else
					_persister.ClearDetails(readModel);
					
			}
			readModel.Model.IsInAdherence = @event.InAdherence;
			readModel.Model.Details.Add(detailModel);
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
						Details = new List<AdherenceDetailModel>
						{
							new AdherenceDetailModel
							{
								LastStateChangedTime = @event.Timestamp
							}
						}
					}
				};
				_persister.Add(model);

				if (_performanceCounter!= null && _performanceCounter.IsEnabled)
					_liteTransactionSyncronization.OnSuccessfulTransaction(() => _performanceCounter.Count());

				return;
			}

			var existingModel = readModel.Model.Details.LastOrDefault();
			if (existingModel == null)
			{
				var detailModel = new AdherenceDetailModel
				{
					LastStateChangedTime = @event.Timestamp
				};
				readModel.Model.Details.Add(detailModel);
			}
			else
			{
				if (readModel.Model.HasShiftEnded)
				{
					readModel.Model.ActualEndTime = calculateActualEndTimeWhenActivityEnds(readModel, @event);
				}
				else
				{
					if (shiftHasStarted(existingModel))
						updateAdherence(readModel, existingModel, @event.Timestamp);

					if (lateForActivity(existingModel, @event))
						existingModel.ActualStartTime = @event.Timestamp;

					existingModel.LastStateChangedTime = @event.Timestamp;
					readModel.Model.IsInAdherence = @event.InAdherence;
					readModel.Model.ActualEndTime = calculateActualEndTimeBeforeActivityEnds(readModel, @event);
				}
				_persister.Update(readModel);

				if (_performanceCounter != null && _performanceCounter.IsEnabled)
					_liteTransactionSyncronization.OnSuccessfulTransaction(() => _performanceCounter.Count());
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
			var lastModel = readModel.Model.Details.LastOrDefault();
			if (lastModel == null)
				return;
			
			updateAdherence(readModel, lastModel, @event.ShiftEndTime);
			readModel.Model.HasShiftEnded = true;
			readModel.Model.ShiftEndTime = @event.ShiftEndTime;
			_persister.Update(readModel);
		}

		private static bool lateForActivity(AdherenceDetailModel model, PersonStateChangedEvent @event)
		{
			return (@event.InAdherence && model.ActualStartTime == null);
		}

		private static DateTime? calculateActualStartTime(AdherenceDetailsReadModel model, AdherenceDetailModel detailModel, PersonActivityStartEvent @event)
		{
			if (@event.InAdherence && model.Model.IsInAdherence)
			{
				return @event.StartTime;
			}
			if (@event.InAdherence && !model.Model.IsInAdherence)
			{
				return detailModel.LastStateChangedTime;
			}
			return null;
		}

		private static bool shiftHasStarted(AdherenceDetailModel model)
		{
			return model.Name != null;
		}

		private static void updateAdherence(AdherenceDetailsReadModel model, AdherenceDetailModel detailModel, DateTime timestamp)
		{
			var timeToAdd = detailModel.LastStateChangedTime.HasValue
				? timestamp - detailModel.LastStateChangedTime.Value
				: timestamp - detailModel.StartTime;

			if (model.Model.IsInAdherence)
				detailModel.TimeInAdherence += timeToAdd.Value;
			else
				detailModel.TimeOutOfAdherence += timeToAdd.Value;
		}

		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}
	}
}