using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public class AgentAdherenceDayAdjustAdherenceToNeutral : IAgentAdherenceDay
	{
		private readonly DateTime _now;
		private readonly DateTimePeriod _fullDay;
		private readonly Func<DateTimePeriod?> _shiftFromSchedule;

		private DateTimePeriod? _collectedShift;
		private readonly IList<OpenPeriod> _collectedApprovedPeriods = new List<OpenPeriod>();
		private readonly IList<OpenPeriod> _collectedAdjustedToNeutralPeriods = new List<OpenPeriod>();
		private readonly IList<HistoricalChangeModel> _collectedChanges = new List<HistoricalChangeModel>();

		private DateTimePeriod? _displayPeriod;
		private IEnumerable<AdherencePeriod> _recordedOutOfAdherences;
		private IEnumerable<AdherencePeriod> _outOfAdherences;
		private IEnumerable<AdherencePeriod> _recordedNeutralAdherences;
		private IEnumerable<AdherencePeriod> _neutralAdherences;
		private IEnumerable<AdherencePeriod> _approvedPeriods;
		private IEnumerable<AdherencePeriod> _adjustedToNeutralAdherences;
		private IEnumerable<HistoricalChangeModel> _changesWithinDisplayPeriod;
		private IEnumerable<OpenPeriod> _adjustedToNeutralIntersectingDisplayPeriod;

		private int? _adherencePercentage;
		private int? _secondsInAdherence;
		private int? _secondsOutOfAdherence;

		public AgentAdherenceDayAdjustAdherenceToNeutral(
			DateTime now,
			DateTimePeriod fullDay,
			Func<DateTimePeriod?> shiftFromSchedule
		)
		{
			_fullDay = fullDay;
			_shiftFromSchedule = shiftFromSchedule;
			_now = floorToSeconds(now);
		}

		public DateTimePeriod DisplayPeriod() => _displayPeriod.GetValueOrDefault();
		public IEnumerable<HistoricalChangeModel> Changes() => _changesWithinDisplayPeriod;
		public IEnumerable<AdherencePeriod> RecordedOutOfAdherences() => _recordedOutOfAdherences.ToArray();
		public IEnumerable<AdherencePeriod> RecordedNeutralAdherences() => _recordedNeutralAdherences.ToArray();

		public IEnumerable<AdherencePeriod> ApprovedPeriods() => _approvedPeriods;
		public IEnumerable<AdherencePeriod> OutOfAdherences() => _outOfAdherences;
		public IEnumerable<AdherencePeriod> NeutralAdherences() => _neutralAdherences;

		public IEnumerable<AdherencePeriod> AdjustedToNeutralAdherences() => _adjustedToNeutralAdherences;

		public int? Percentage() => _adherencePercentage;
		public int? SecondsInAdherence() => _secondsInAdherence;
		public int? SecondsOutOfAdherence() => _secondsOutOfAdherence;

		public void Apply(PersonShiftStartEvent @event) => applyShift(@event);
		public void Apply(PersonShiftEndEvent @event) => applyShift(@event);
		public void Apply(PersonAdherenceDayStartEvent @event) => applySolidProof(@event);
		public void Apply(PersonStateChangedEvent @event) => applySolidProof(@event);
		public void Apply(PersonRuleChangedEvent @event) => applySolidProof(@event);

		public void Apply(PersonArrivedLateForWorkEvent @event)
		{
			var model = applySolidProof(@event);
			if (model != null)
				model.LateForWorkMinutes = (int) Math.Round(new DateTimePeriod(@event.ShiftStart, @event.Timestamp).ElapsedTime().TotalMinutes);
		}

		public void Apply(PeriodApprovedAsInAdherenceEvent @event) =>
			_collectedApprovedPeriods.Add(new OpenPeriod {StartTime = @event.StartTime, EndTime = @event.EndTime});

		public void Apply(ApprovedPeriodRemovedEvent @event) =>
			_collectedApprovedPeriods.Remove(new OpenPeriod {StartTime = @event.StartTime, EndTime = @event.EndTime});

		public void Apply(PeriodAdjustedToNeutralEvent @event) =>
			_collectedAdjustedToNeutralPeriods.Add(new OpenPeriod {StartTime = @event.StartTime, EndTime = @event.EndTime});

		public void ApplyDone()
		{
			var shift = determineShift(_collectedShift, _shiftFromSchedule);
			_displayPeriod = determineDisplayPeriod(_fullDay, shift);		
			_changesWithinDisplayPeriod = changesWithinDisplayPeriod(_collectedChanges, _displayPeriod.Value);

			_adjustedToNeutralIntersectingDisplayPeriod = _collectedAdjustedToNeutralPeriods.Intersects(new OpenPeriod(_displayPeriod.Value.StartDateTime, _displayPeriod.Value.EndDateTime));

			var (recordedOutOfAdherences, recordedNeutralAdherences) = recordedAdherences();
			var (projectedOutOfAdherences, projectedNeutralAdherences) = projectedAdherences(recordedOutOfAdherences, recordedNeutralAdherences);

			buildOutputPeriods(recordedOutOfAdherences, recordedNeutralAdherences, projectedOutOfAdherences, projectedNeutralAdherences);
			calculateAdherence(shift, recordedOutOfAdherences, recordedNeutralAdherences);
		}

		private void applyShift(dynamic @event) =>
			_collectedShift = new DateTimePeriod(@event.ShiftStartTime, @event.ShiftEndTime);

		private HistoricalChangeModel applySolidProof(ISolidProof @event)
		{
			// move duplication check and duration sum to calculate?
			var lastProof = _collectedChanges.LastOrDefault();
			if (lastProof != null && !isSameProof(lastProof, @event))
				lastProof.Duration = floorToSeconds(@event.Timestamp).Subtract(floorToSeconds(lastProof.Timestamp)).ToString();

			var needNewProof = (lastProof == null || !isSameProof(lastProof, @event));
			if (needNewProof)
			{
				var proof = createProof(@event);
				_collectedChanges.Add(proof);
				return proof;
			}

			lastProof.Timestamp = @event.Timestamp;
			return lastProof;
		}

		private static HistoricalChangeModel createProof(ISolidProof @event)
		{
			return new HistoricalChangeModel
			{
				StateName = @event.StateName,
				ActivityName = @event.ActivityName,
				ActivityColor = @event.ActivityColor,
				RuleName = @event.RuleName,
				RuleColor = @event.RuleColor,
				Adherence = convertAdherence(@event.Adherence),
				Timestamp = @event.Timestamp
			};
		}

		private static bool isSameProof(HistoricalChangeModel model, ISolidProof @event) =>
			model.Timestamp == @event.Timestamp &&
			model.ActivityName == @event.ActivityName &&
			model.ActivityColor == @event.ActivityColor &&
			model.StateName == @event.StateName &&
			model.RuleColor == @event.RuleColor &&
			model.RuleName == @event.RuleName &&
			model.Adherence == convertAdherence(@event.Adherence);


		private void buildOutputPeriods(
			IEnumerable<OpenPeriod> recordedOutOfAdherences,
			IEnumerable<OpenPeriod> recordedNeutralAdherences,
			IEnumerable<OpenPeriod> projectedOutOfAdherences,
			IEnumerable<OpenPeriod> projectedNeutralAdherences
		)
		{
			_recordedOutOfAdherences = recordedOutOfAdherences.ToAdherencePeriods();
			_recordedNeutralAdherences = recordedNeutralAdherences.ToAdherencePeriods();
			_approvedPeriods = _collectedApprovedPeriods.ToAdherencePeriods();
			_adjustedToNeutralAdherences = _adjustedToNeutralIntersectingDisplayPeriod.ToAdherencePeriods();
			_outOfAdherences = projectedOutOfAdherences.ToAdherencePeriods();
			_neutralAdherences = projectedNeutralAdherences.ToAdherencePeriods();
		}

		private void calculateAdherence(DateTimePeriod? shift, IEnumerable<OpenPeriod> recordedOutOfAdherences, IEnumerable<OpenPeriod> recordedNeutralAdherences)
		{
			if (!shift.HasValue)
				return;

			_secondsInAdherence = calculateSecondsInAdherence(shift.Value, _now, recordedOutOfAdherences, recordedNeutralAdherences, _adjustedToNeutralIntersectingDisplayPeriod, _collectedApprovedPeriods);
			_secondsOutOfAdherence = calculateSecondsOutOfAdherence(shift.Value, recordedOutOfAdherences, _adjustedToNeutralIntersectingDisplayPeriod, _collectedApprovedPeriods);
			_adherencePercentage = AdherencePercentageCalculation.Calculate(_secondsInAdherence, _secondsOutOfAdherence);
		}

		private (IEnumerable<OpenPeriod>, IEnumerable<OpenPeriod>) recordedAdherences()
		{
			var recordedOutOfAdherences = buildAdherences(_displayPeriod.Value, _now, _collectedChanges, HistoricalChangeAdherence.Out).ToArray();
			var recordedNeutralAdherences = buildAdherences(_displayPeriod.Value, _now, _collectedChanges, HistoricalChangeAdherence.Neutral).ToArray();
			return (recordedOutOfAdherences, recordedNeutralAdherences);
		}

		private (IEnumerable<OpenPeriod>, IEnumerable<OpenPeriod>) projectedAdherences(
			IEnumerable<OpenPeriod> recordedOutOfAdherences,
			IEnumerable<OpenPeriod> recordedNeutralAdherences
		)
		{
			var projectedOutOfAdherences = recordedOutOfAdherences
				.Subtract(_collectedApprovedPeriods)
				.Subtract(_adjustedToNeutralIntersectingDisplayPeriod)
				.ToArray();
			var projectedNeutralAdherences = recordedNeutralAdherences
				.Subtract(_adjustedToNeutralIntersectingDisplayPeriod)
				.Concat(_adjustedToNeutralIntersectingDisplayPeriod)
				.Subtract(_collectedApprovedPeriods)
				.ToArray();
			return (projectedOutOfAdherences, projectedNeutralAdherences);
		}

		private static DateTimePeriod? determineShift(DateTimePeriod? shift, Func<DateTimePeriod?> shiftFromSchedule) =>
			shift ?? shiftFromSchedule.Invoke();

		private static DateTimePeriod determineDisplayPeriod(DateTimePeriod fullDay, DateTimePeriod? shift) =>
			shift == null ? fullDay : new DateTimePeriod(shift.Value.StartDateTime.AddHours(-1), shift.Value.EndDateTime.AddHours(1));

		private static IEnumerable<HistoricalChangeModel> changesWithinDisplayPeriod(IEnumerable<HistoricalChangeModel> changes, DateTimePeriod displayPeriod) =>
			changes
				.Where(x => x.Timestamp >= displayPeriod.StartDateTime && x.Timestamp <= displayPeriod.EndDateTime)
				.ToArray();

		private static IEnumerable<OpenPeriod> buildAdherences(DateTimePeriod displayPeriod, DateTime now, IEnumerable<HistoricalChangeModel> changes, HistoricalChangeAdherence adherence)
		{
			var adherences = buildPeriods(adherence, changes, displayPeriod);
			closeOpenPeriod(displayPeriod, now, adherences);
			adherences = removeTinyPeriods(adherences);
			adherences = mergeAdjacentPeriods(adherences);
			adherences = adherences.Intersects(new OpenPeriod(displayPeriod.StartDateTime, displayPeriod.EndDateTime));
			return adherences;
		}

		private static IEnumerable<OpenPeriod> buildPeriods(HistoricalChangeAdherence adherence, IEnumerable<HistoricalChangeModel> changes, DateTimePeriod period)
		{
			var result = changes
				.Select(change => new
				{
					Timestamp = floorToSeconds(change.Timestamp),
					periodStarting = change.Adherence == adherence
				})
				.TransitionsOf(x => x.periodStarting)
				.Aggregate(new List<OpenPeriod>(), (periods, change) =>
				{
					if (change.periodStarting)
					{
						DateTime? startTime = null;
						if (change.Timestamp > period.StartDateTime)
							startTime = change.Timestamp;
						periods.Add(new OpenPeriod(startTime, null));
					}
					else if (periods.Any())
						periods.Last().EndTime = change.Timestamp;

					return periods;
				});

			return result;
		}

		private static void closeOpenPeriod(DateTimePeriod displayPeriod, DateTime now, IEnumerable<OpenPeriod> periods)
		{
			var period = periods.LastOrDefault();
			if (period == null) return;
			if (period.EndTime != null) return;
			if (now > displayPeriod.EndDateTime) return;
			period.EndTime = now;
		}

		private static IEnumerable<OpenPeriod> removeTinyPeriods(IEnumerable<OpenPeriod> periods) =>
			(from p in periods
				let openStart = p.StartTime == null
				let openEnd = p.EndTime == null
				let isNotTiny = p.EndTime - p.StartTime >= TimeSpan.FromSeconds(1)
				where openStart || openEnd || isNotTiny
				select p
			).ToArray();

		private static IEnumerable<OpenPeriod> mergeAdjacentPeriods(IEnumerable<OpenPeriod> periods) =>
			periods.Aggregate(new List<OpenPeriod>(), (acc, i) =>
			{
				var lastPeriod = acc.LastOrDefault();
				if (lastPeriod != null && lastPeriod.EndTime.Value == i.StartTime)
					lastPeriod.EndTime = i.EndTime;
				else
					acc.Add(i);
				return acc;
			});


		private static int? calculateSecondsInAdherence(
			DateTimePeriod shift,
			DateTime now,
			IEnumerable<OpenPeriod> recordedOutOfAdherences,
			IEnumerable<OpenPeriod> recordedNeutralAdherences,
			IEnumerable<OpenPeriod> adjustedToNeutral,
			IEnumerable<OpenPeriod> approvedPeriods
		)
		{
			var normalizedAdjustedPeriods = adjustedToNeutral.MergeIntersecting().ToArray();
			var outOfAdherences = recordedOutOfAdherences
				.Subtract(approvedPeriods)
				.Subtract(normalizedAdjustedPeriods)
				.ToArray();
			var neutralAdherences = recordedNeutralAdherences
				.Subtract(normalizedAdjustedPeriods)
				.Concat(normalizedAdjustedPeriods)
				.Subtract(approvedPeriods)
				.ToArray();

			var timeOut = timeInShift(shift, outOfAdherences);
			var timeNeutral = timeInShift(shift, neutralAdherences);

			var calculateUntil = new[] {now, shift.EndDateTime}.Min();
			var shiftTime = calculateUntil - shift.StartDateTime;
			var timeIn = shiftTime - timeOut - timeNeutral;

			return Convert.ToInt32(timeIn.TotalSeconds);
		}

		private static int? calculateSecondsOutOfAdherence(
			DateTimePeriod shift,
			IEnumerable<OpenPeriod> recordedOutOfAdherences,
			IEnumerable<OpenPeriod> adjustedToNeutral,
			IEnumerable<OpenPeriod> approvedPeriods
		)
		{
			var outOfAdherences = recordedOutOfAdherences
				.Subtract(approvedPeriods)
				.Subtract(adjustedToNeutral)
				.ToArray();
			var timeOut = timeInShift(shift, outOfAdherences);
			return Convert.ToInt32(timeOut.TotalSeconds);
		}

		private static TimeSpan timeInShift(DateTimePeriod shift, IEnumerable<OpenPeriod> periods)
		{
			var shiftAsOpenPeriod = new OpenPeriod(shift.StartDateTime, shift.EndDateTime);
			var seconds = from p in periods
				let intersection = shiftAsOpenPeriod.Intersection(p)
				where intersection != null
				let t = intersection.EndTime - intersection.StartTime
				select t.Value.TotalSeconds;

			return TimeSpan.FromSeconds(seconds.Sum());
		}

		private static DateTime floorToSeconds(DateTime dateTime) => dateTime.Truncate(TimeSpan.FromSeconds(1));

		private static HistoricalChangeAdherence? convertAdherence(EventAdherence? adherence) =>
			adherence != null ? (HistoricalChangeAdherence?) Enum.Parse(typeof(HistoricalChangeAdherence), adherence.ToString()) : null;
	}
}