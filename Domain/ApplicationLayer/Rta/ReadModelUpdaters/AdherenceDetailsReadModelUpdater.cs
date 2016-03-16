using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[UseOnToggle(Toggles.RTA_AdherenceDetails_34267)]
	public class AdherenceDetailsReadModelUpdater :
		IRunOnHangfire,
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonShiftEndEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IInitializeble
	{
		private readonly IAdherenceDetailsReadModelPersister _persister;
		private readonly IReadModelTransactionSyncronization _readModelTransactionSyncronization;
		private readonly IPerformanceCounter _performanceCounter;
		private readonly IComparer<AdherenceDetailsReadModelAdherence> _adherenceComparer;

		public AdherenceDetailsReadModelUpdater(
			IAdherenceDetailsReadModelPersister persister,
			IReadModelTransactionSyncronization readModelTransactionSyncronization,
			IPerformanceCounter performanceCounter)
		{
			_persister = persister;
			_readModelTransactionSyncronization = readModelTransactionSyncronization;
			_performanceCounter = performanceCounter;
			_adherenceComparer = new AdherenceStateComparer_DeclarationOrderIsNotToBeTrusted();
		}

		private static AdherenceDetailsReadModelAdherence adherenceFor(PersonStateChangedEvent @event)
		{
			if (@event.Adherence == EventAdherence.In)
				return AdherenceDetailsReadModelAdherence.In;
			if (@event.Adherence == EventAdherence.Out)
				return AdherenceDetailsReadModelAdherence.Out;
			return AdherenceDetailsReadModelAdherence.Neutral;
		}

		private static AdherenceDetailsReadModelAdherence adherenceFor(PersonActivityStartEvent @event)
		{
			if (@event.Adherence == EventAdherence.In)
				return AdherenceDetailsReadModelAdherence.In;
			if (@event.Adherence == EventAdherence.Out)
				return AdherenceDetailsReadModelAdherence.Out;
			return AdherenceDetailsReadModelAdherence.Neutral;
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
					addAdherence(s, @event.StartTime, adherenceFor(@event), false);
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
					addAdherence(s, @event.Timestamp, adherenceFor(@event), true);

					if (s.ShiftEndTime.HasValue && !s.FirstStateChangeOutOfAdherenceWithLastActivity.HasValue)
					{
						if (@event.AdherenceWithPreviousActivity == EventAdherence.Out)
							s.FirstStateChangeOutOfAdherenceWithLastActivity = @event.Timestamp;
					}
				});

			if (_performanceCounter != null && _performanceCounter.IsEnabled)
				_readModelTransactionSyncronization.OnSuccessfulTransaction(() => _performanceCounter.Count());
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

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		[ReadModelUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}


		private void handleEvent(Guid personId, DateOnly date, Action<AdherenceDetailsReadModelState> mutate)
		{
			var model = _persister.Get(date, personId)
						?? new AdherenceDetailsReadModel
						{
							Date = date.Date,
							PersonId = personId,
							State = new AdherenceDetailsReadModelState()
						};
			mutate(model.State);
			calculate(model);
			_persister.Persist(model);
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

		private void addAdherence(AdherenceDetailsReadModelState model, DateTime time, AdherenceDetailsReadModelAdherence adherence, bool stateChanged)
		{
			model.Adherence = model.Adherence
				.Concat(new[]
				{
					new AdherenceDetailsReadModelAdherenceState
					{
						Time = time,
						Adherence = adherence,
						StateChanged = stateChanged
					}
				})
				.OrderBy(x => x.Time)
				.ThenBy(x => x.Adherence, _adherenceComparer)
				.ToArray()
				;

			removeDuplicateAdherences(model);
			removeRedundantOldAdherences(model);
		}

		private void removeDuplicateAdherences(AdherenceDetailsReadModelState model)
		{
			var duplicateByTime = model.Adherence
				.Where(x => model.Adherence.Count(a => a.Time == x.Time) > 1)
				.GroupBy(x => x.Time)
				.Select(x => x.OrderBy(y => y.Adherence, _adherenceComparer).ExceptLast())
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
			var cleaned = safeToRemove.TransitionsOf(x => x.Adherence);
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
				.TransitionsOf(x => x.Adherence)
				.LastOrDefault(x =>
					x.Time <= model.State.ShiftEndTime &&
					x.Time >= model.State.Activities.Last().StartTime
				);

			if (endingAdherenceOfLastActivity != null)
				if (endingAdherenceOfLastActivity.Adherence == AdherenceDetailsReadModelAdherence.Out)
					return endingAdherenceOfLastActivity.Time;

			if (model.State.FirstStateChangeOutOfAdherenceWithLastActivity.HasValue)
				return model.State.FirstStateChangeOutOfAdherenceWithLastActivity.Value;

			return null;
		}

		private static bool? calculateLastAdherence(AdherenceDetailsReadModel model)
		{
			if (!model.State.Adherence.IsEmpty())
			{
				if (model.State.Adherence.Last().Adherence == AdherenceDetailsReadModelAdherence.In)
					return true;
				if (model.State.Adherence.Last().Adherence == AdherenceDetailsReadModelAdherence.Out)
					return false;
			}
			return null;
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
			var adherenceTransitions = model.State.Adherence.TransitionsOf(x => x.Adherence).ToArray();
			var adherenceBefore = adherenceTransitions.LastOrDefault(x => x.Time < activity.StartTime);
			var adherenceChange = adherenceTransitions.SingleOrDefault(x => x.Time == activity.StartTime);
			var adherenceAfter = adherenceTransitions.FirstOrDefault(x => x.Time > activity.StartTime);

			if (adherenceChange != null)
			{
				if (adherenceBefore != null)
				{
					if (fromOutToInOrNeutralAdherence(adherenceBefore.Adherence, adherenceChange.Adherence))
						return adherenceBefore.StateChanged ? adherenceBefore.Time : adherenceChange.Time;

					if (fromInToNeutralAdherence(adherenceBefore.Adherence, adherenceChange.Adherence))
						return adherenceChange.Time;

					if (fromNeutralToInAdherence(adherenceBefore.Adherence, adherenceChange.Adherence))
						return adherenceChange.Time;
				}

				if (adherenceAfter != null)
				{
					if (fromOutToInOrNeutralAdherence(adherenceChange.Adherence, adherenceAfter.Adherence))
						return adherenceAfter.StateChanged ? adherenceAfter.Time : (DateTime?) null;

					if (fromNeutralToInAdherence(adherenceChange.Adherence, adherenceAfter.Adherence))
						return adherenceAfter.Time;

					if (fromInToNeutralAdherence(adherenceChange.Adherence, adherenceAfter.Adherence))
						return adherenceAfter.Time;
				}


				if (adherenceBefore == null)
					if (isInOrNeutralAdherence(adherenceChange.Adherence))
						return adherenceChange.Time;
			}

			if (adherenceChange == null && adherenceBefore != null)
				if (isInOrNeutralAdherence(adherenceBefore.Adherence))
					return activity.StartTime;

			return null;
		}

		private static bool isInOrNeutralAdherence(AdherenceDetailsReadModelAdherence adherence)
		{
			return adherence == AdherenceDetailsReadModelAdherence.In ||
			       adherence == AdherenceDetailsReadModelAdherence.Neutral;
		}

		private static bool fromOutToInOrNeutralAdherence(AdherenceDetailsReadModelAdherence from, AdherenceDetailsReadModelAdherence to)
		{
			if (from == AdherenceDetailsReadModelAdherence.Out)
				if (to == AdherenceDetailsReadModelAdherence.In ||
					to == AdherenceDetailsReadModelAdherence.Neutral)
					return true;
			return false;
		}

		private static bool fromNeutralToInAdherence(AdherenceDetailsReadModelAdherence from, AdherenceDetailsReadModelAdherence to)
		{
			if (from == AdherenceDetailsReadModelAdherence.Neutral)
				if (to == AdherenceDetailsReadModelAdherence.In)
					return true;
			return false;
		}

		private static bool fromInToNeutralAdherence(AdherenceDetailsReadModelAdherence from, AdherenceDetailsReadModelAdherence to)
		{
			if (from == AdherenceDetailsReadModelAdherence.In)
				if (to == AdherenceDetailsReadModelAdherence.Neutral)
					return true;
			return false;
		}

		private static TimeSpan? calculateInAdherenceForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			var containsNeutralAdherence = false;
			var inAdherenceForActivity = adherenceForActivity(model, activity)
				.WithPrevious()
				.Aggregate(TimeSpan.Zero, (current, a) =>
				{
					if (a.Previous.Adherence == AdherenceDetailsReadModelAdherence.Neutral)
						containsNeutralAdherence = true;
					if (a.Previous.Adherence == AdherenceDetailsReadModelAdherence.In)
						return current + (a.This.Time - a.Previous.Time);
					return current;
				});
			if (containsNeutralAdherence && inAdherenceForActivity == TimeSpan.Zero)
				return null;
			return inAdherenceForActivity;
		}

		private static TimeSpan? calculateOutOfAdherenceForActivity(AdherenceDetailsReadModel model, AdherenceDetailsReadModelActivityState activity)
		{
			var containsNeutralAdherence = false;
			var outOfAdherenceForActivity = adherenceForActivity(model, activity)
				.WithPrevious()
				.Aggregate(TimeSpan.Zero, (current, a) =>
				{
					if (a.Previous.Adherence == AdherenceDetailsReadModelAdherence.Neutral)
						containsNeutralAdherence = true;
					if (a.Previous.Adherence == AdherenceDetailsReadModelAdherence.Out)
						return current + (a.This.Time - a.Previous.Time);
					return current;
				});
			if (containsNeutralAdherence && outOfAdherenceForActivity == TimeSpan.Zero)
				return null;
			return outOfAdherenceForActivity;
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
					Adherence = adherenceBefore.Adherence
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

	/// <summary>
	/// And specifying order is just ugly
	/// Might be wrong, edge case stuff
	/// </summary>
	public class AdherenceStateComparer_DeclarationOrderIsNotToBeTrusted : IComparer<AdherenceDetailsReadModelAdherence>
	{
		public int Compare(AdherenceDetailsReadModelAdherence x, AdherenceDetailsReadModelAdherence y)
		{

			if (x == y) return 0;

			const int before = -1;
			const int after = 1;
			if (x == AdherenceDetailsReadModelAdherence.Out && y == AdherenceDetailsReadModelAdherence.In)
				return before;
			if (x == AdherenceDetailsReadModelAdherence.Out && y == AdherenceDetailsReadModelAdherence.Neutral)
				return before;

			if (x == AdherenceDetailsReadModelAdherence.Neutral && y == AdherenceDetailsReadModelAdherence.Out)
				return after;
			if (x == AdherenceDetailsReadModelAdherence.Neutral && y == AdherenceDetailsReadModelAdherence.In)
				return before;

			if (x == AdherenceDetailsReadModelAdherence.In && y == AdherenceDetailsReadModelAdherence.Out)
				return after;
			if (x == AdherenceDetailsReadModelAdherence.In && y == AdherenceDetailsReadModelAdherence.Neutral)
				return after;

			return 0;
		}
	}
}
