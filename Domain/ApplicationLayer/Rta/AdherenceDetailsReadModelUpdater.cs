using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Collection;
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

		public AdherenceDetailsReadModelUpdater(
			IAdherenceDetailsReadModelPersister persister,
			ILiteTransactionSyncronization liteTransactionSyncronization,
			IPerformanceCounter performanceCounter)
		{
			_persister = persister;
			_liteTransactionSyncronization = liteTransactionSyncronization;
			_performanceCounter = performanceCounter;
		}

		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonActivityStartEvent @event)
		{
			var date = @event.BelongsToDate.HasValue ? @event.BelongsToDate.Value : new DateOnly(@event.StartTime);
			handleEvent(
				@event.PersonId,
				date,
				s =>
				{
					incrementLastUpdate(s, @event.StartTime);
					addActivity(s, @event.StartTime, @event.Name);
					addAdherence(s, @event.StartTime, @event.InAdherence);
				});
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonStateChangedEvent @event)
		{
			var date = @event.BelongsToDate.HasValue ? @event.BelongsToDate.Value : new DateOnly(@event.Timestamp);
			handleEvent(
				@event.PersonId,
				date,
				s =>
				{
					incrementLastUpdate(s, @event.Timestamp);
					addAdherence(s, @event.Timestamp, @event.InAdherence);
					if (s.ShiftEndTime.HasValue)
						if (!s.FirstStateChangeOutOfAdherenceWithLastActivity.HasValue)
							if (!@event.InAdherenceWithPreviousActivity)
								s.FirstStateChangeOutOfAdherenceWithLastActivity = @event.Timestamp;
				});

			if (_performanceCounter != null && _performanceCounter.IsEnabled)
				_liteTransactionSyncronization.OnSuccessfulTransaction(() => _performanceCounter.Count());
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonShiftEndEvent @event)
		{
			var date = @event.BelongsToDate.HasValue ? @event.BelongsToDate.Value : new DateOnly(@event.ShiftStartTime);
			handleEvent(
				@event.PersonId,
				date,
				s =>
				{
					incrementLastUpdate(s, @event.ShiftEndTime);
					s.ShiftEndTime = @event.ShiftEndTime;
				});
		}

		private void handleEvent(Guid personId, DateOnly date, Action<AdherenceDetailsReadModelState> mutate)
		{
			var model = _persister.Get(personId, date);
			if (model == null)
			{
				model = new AdherenceDetailsReadModel
				{
					Date = date,
					PersonId = personId,
					State = new AdherenceDetailsReadModelState()
				};
				mutate(model.State);
				calculate(model);
				_persister.Add(model);
			}
			else
			{
				mutate(model.State);
				calculate(model);
				_persister.Update(model);
			}
		}

		private static void incrementLastUpdate(AdherenceDetailsReadModelState model, DateTime time)
		{
			if (model.LastUpdate < time)
				model.LastUpdate = time;
		}

		private static void addActivity(AdherenceDetailsReadModelState model, DateTime time, string name)
		{
			model.Activities = model.Activities
				.Concat(new[]
				{
					new AdherenceDetailsReadModelActivityState
					{
						StartTime = time,
						Name = name,
					}
				})
				.OrderBy(x => x.StartTime)
				.ToArray();
		}

		private static void addAdherence(AdherenceDetailsReadModelState model, DateTime time, bool inAdherence)
		{
			model.Adherence = model.Adherence
				.Concat(new[]
				{
					new AdherenceDetailsReadModelAdherenceState
					{
						Time = time,
						InAdherence = inAdherence,
					}
				})
				.OrderBy(x => x.Time)
				.ThenBy(x => x.InAdherence)
				.ToArray()
				;

			removeDuplicateAdherences(model);
			removeRedundantOldAdherences(model);
		}

		private static void removeDuplicateAdherences(AdherenceDetailsReadModelState model)
		{
			var duplicateByTime = model.Adherence
				.Where(x => model.Adherence.Count(a => a.Time == x.Time) > 1)
				.GroupBy(x => x.Time)
				.Select(x => x.OrderBy(y => y.InAdherence).ExceptLast())
				.SelectMany(x => x)
				.ToArray()
				;

			model.Adherence = model.Adherence.Where(a => !duplicateByTime.Contains(a)).ToArray();
		}

		private static void removeRedundantOldAdherences(AdherenceDetailsReadModelState model)
		{
			if (model.LastUpdate == DateTime.MinValue)
				return;
			var safeToRemoveOlderThan = model.LastUpdate.Subtract(TimeSpan.FromMinutes(10));
			var safeToRemove = model.Adherence.Where(x => x.Time < safeToRemoveOlderThan).ToArray();
			var keep = model.Adherence.Except(safeToRemove);
			var cleaned = safeToRemove.TransitionsOf(x => x.InAdherence);
			model.Adherence = cleaned.Concat(keep).ToArray();
		}

		private static void calculate(AdherenceDetailsReadModel model)
		{
			model.Model = new AdherenceDetailsModel
			{
				ShiftEndTime = model.State.ShiftEndTime,
				ActualEndTime = calculateActualEndTimeForShift(model),
				LastAdherence = calculateLastAdherence(model),
				LastUpdate = model.State.LastUpdate,
				Activities = model.State.Activities.Select(s => calculateActivity(model, s)).ToArray()
			};
		}

		private static DateTime? calculateActualEndTimeForShift(AdherenceDetailsReadModel model)
		{
			if (!model.State.ShiftEndTime.HasValue)
				return null;
			if (model.State.Activities.IsEmpty())
				return null;

			var endingAdherenceOfLastActivity = model.State.Adherence
				.TransitionsOf(x => x.InAdherence)
				.LastOrDefault(x =>
					x.Time <= model.State.ShiftEndTime &&
					x.Time >= model.State.Activities.Last().StartTime
				);

			if (endingAdherenceOfLastActivity != null)
				if (!endingAdherenceOfLastActivity.InAdherence)
					return endingAdherenceOfLastActivity.Time;

			if (model.State.FirstStateChangeOutOfAdherenceWithLastActivity.HasValue)
				return model.State.FirstStateChangeOutOfAdherenceWithLastActivity.Value;

			return null;
		}

		private static bool calculateLastAdherence(AdherenceDetailsReadModel model)
		{
			return !model.State.Adherence.IsEmpty() && model.State.Adherence.Last().InAdherence;
		}

		private static ActivityAdherence calculateActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			return new ActivityAdherence
			{
				Name = activity.Name,
				StartTime = activity.StartTime,
				ActualStartTime = calculateActualStartTimeForActivity(model, activity),
				TimeInAdherence = calculateInAdherenceForActivity(model, activity),
				TimeOutOfAdherence = calculateOutOfAdherenceForActivity(model, activity)
			};
		}

		private static DateTime? calculateActualStartTimeForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			var adherenceTransitions = model.State.Adherence.TransitionsOf(x => x.InAdherence).ToArray();
			var adherenceBefore = adherenceTransitions.LastOrDefault(x => x.Time < activity.StartTime);
			var adherenceChange = adherenceTransitions.SingleOrDefault(x => x.Time == activity.StartTime);
			var adherenceAfter = adherenceTransitions.FirstOrDefault(x => x.Time > activity.StartTime);

			if (adherenceChange != null && adherenceAfter != null)
				if (!adherenceChange.InAdherence && adherenceAfter.InAdherence)
					return adherenceAfter.Time;

			if (adherenceChange != null && adherenceBefore != null)
				if (adherenceChange.InAdherence && !adherenceBefore.InAdherence)
					return adherenceBefore.Time;

			if (adherenceChange != null && adherenceBefore == null)
				if (adherenceChange.InAdherence)
					return adherenceChange.Time;

			if (adherenceChange == null && adherenceBefore != null)
				if (adherenceBefore.InAdherence)
					return activity.StartTime;

			return null;
		}

		private static TimeSpan calculateInAdherenceForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			var things = adherenceForActivity(model, activity);
			var result = things
				.WithPrevious()
				.Aggregate(TimeSpan.Zero, (current, a) =>
				{
					if (a.Previous.InAdherence)
						return current + (a.This.Time - a.Previous.Time);
					return current;
				});
			return result;
		}

		private static TimeSpan calculateOutOfAdherenceForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			return adherenceForActivity(model, activity)
				.WithPrevious()
				.Aggregate(TimeSpan.Zero, (current, a) =>
				{
					if (!a.Previous.InAdherence)
						return current + (a.This.Time - a.Previous.Time);
					return current;
				});
		}

		private static IEnumerable<AdherenceDetailsReadModelAdherenceState> adherenceForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			var nextActivity = model.State.Activities
				.SkipWhile(a => a != activity)
				.Skip(1)
				.FirstOrDefault();

			var fromTime = activity.StartTime;
			DateTime toTime;
			if (nextActivity != null)
				toTime = nextActivity.StartTime;
			else if (model.State.ShiftEndTime.HasValue)
				toTime = model.State.ShiftEndTime.Value;
			else
				toTime = model.State.LastUpdate;

			var adherence = (
				from a in model.State.Adherence
				let afterActivityStart = a.Time >= fromTime
				let beforeActivityEnd = a.Time <= toTime
				where
					afterActivityStart && beforeActivityEnd
				select a
				).ToArray();

			if (!adherence.Any(x => x.Time == fromTime))
			{
				var adherenceBefore = model.State.Adherence.LastOrDefault(x => x.Time < fromTime);
				var activityStartAdherence = new AdherenceDetailsReadModelAdherenceState
				{
					Time = fromTime,
					InAdherence = adherenceBefore.InAdherence
				};
				adherence = new[] { activityStartAdherence }.Concat(adherence).ToArray();
			}

			if (!adherence.Any(x => x.Time == toTime))
			{
				var activityEndAdherence = new AdherenceDetailsReadModelAdherenceState
				{
					Time = toTime
				};
				adherence = adherence.Concat(new[] { activityEndAdherence }).ToArray();
			}

			return adherence;
		}

	}

}
