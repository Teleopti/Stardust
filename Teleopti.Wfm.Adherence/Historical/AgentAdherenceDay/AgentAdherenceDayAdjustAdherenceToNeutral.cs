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

		private DateTimePeriod? _calculatedDisplayPeriod;
		private IEnumerable<AdherencePeriod> _calculatedRecordedOutOfAdherences;
		private IEnumerable<AdherencePeriod> _calculatedOutOfAdherences;
		private IEnumerable<AdherencePeriod> _calculatedApprovedPeriods;
		private IEnumerable<AdherencePeriod> _calculatedAdjustedToNeutralAdherences;
		private IEnumerable<HistoricalChangeModel> _calculatedChanges;

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

		public DateTimePeriod DisplayPeriod() => _calculatedDisplayPeriod.GetValueOrDefault();
		public IEnumerable<HistoricalChangeModel> Changes() => _calculatedChanges;
		public IEnumerable<AdherencePeriod> RecordedOutOfAdherences() => _calculatedRecordedOutOfAdherences.ToArray();
		public IEnumerable<AdherencePeriod> ApprovedPeriods() => _calculatedApprovedPeriods;
		public IEnumerable<AdherencePeriod> OutOfAdherences() => _calculatedOutOfAdherences;
		public IEnumerable<AdherencePeriod> AdjustedToNeutralAdherences() => _calculatedAdjustedToNeutralAdherences;

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

		public void ApplyDone() => calculate();

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

		private void calculate()
		{
			var shift = calculateShift(_collectedShift, _shiftFromSchedule);
			_calculatedDisplayPeriod = calculatePeriod(_fullDay, shift);
			_calculatedChanges = calculateChanges(_collectedChanges, _calculatedDisplayPeriod.Value);

			var recordedOutOfAdherences = calculateAdherences(_calculatedDisplayPeriod.Value, _now, _collectedChanges, HistoricalChangeAdherence.Out);

			_calculatedRecordedOutOfAdherences = calculateRecordedOutOfAdherences(recordedOutOfAdherences);
			_calculatedOutOfAdherences = calculateOutOfAdherences(recordedOutOfAdherences, _collectedApprovedPeriods);
			_calculatedApprovedPeriods = calculateApprovedPeriods(_collectedApprovedPeriods);
			_calculatedAdjustedToNeutralAdherences = calculateAdjustedToNeutralPeriods(_collectedAdjustedToNeutralPeriods);

			if (!shift.HasValue)
				return;

			var recordedNeutralAdherences = calculateAdherences(_calculatedDisplayPeriod.Value, _now, _collectedChanges, HistoricalChangeAdherence.Neutral);

			_secondsInAdherence = calculateSecondsInAdherence(shift.Value, _now, recordedOutOfAdherences, recordedNeutralAdherences, _collectedAdjustedToNeutralPeriods, _collectedApprovedPeriods);
			_secondsOutOfAdherence = calculateSecondsOutOfAdherence(shift.Value, recordedOutOfAdherences, _collectedAdjustedToNeutralPeriods, _collectedApprovedPeriods);
			_adherencePercentage = AdherencePercentageCalculation.Calculate(_secondsInAdherence, _secondsOutOfAdherence);
		}

		private static DateTimePeriod? calculateShift(DateTimePeriod? shift, Func<DateTimePeriod?> shiftFromSchedule) =>
			shift ?? shiftFromSchedule.Invoke();

		private static DateTimePeriod calculatePeriod(DateTimePeriod fullDay, DateTimePeriod? shift) =>
			shift == null ? fullDay : new DateTimePeriod(shift.Value.StartDateTime.AddHours(-1), shift.Value.EndDateTime.AddHours(1));

		private static IEnumerable<HistoricalChangeModel> calculateChanges(IEnumerable<HistoricalChangeModel> changes, DateTimePeriod displayPeriod) =>
			changes
				.Where(x => x.Timestamp >= displayPeriod.StartDateTime && x.Timestamp <= displayPeriod.EndDateTime)
				.ToArray();

		private static IEnumerable<OpenPeriod> calculateAdherences(DateTimePeriod displayPeriod, DateTime now, IEnumerable<HistoricalChangeModel> changes, HistoricalChangeAdherence adherence)
		{
			var adherences = buildPeriods(adherence, changes, displayPeriod);
			closeOpenPeriod(displayPeriod, now, adherences);
			adherences = removeTinyPeriods(adherences);
			adherences = mergeAdjacentPeriods(adherences);
			adherences = intersectsWithPeriod(displayPeriod, adherences);
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

		private static IEnumerable<AdherencePeriod> calculateRecordedOutOfAdherences(IEnumerable<OpenPeriod> recordedOutOfAdherences) =>
			recordedOutOfAdherences
				.Select(x => new AdherencePeriod(x.StartTime, x.EndTime))
				.ToArray();

		private static IEnumerable<AdherencePeriod> calculateOutOfAdherences(IEnumerable<OpenPeriod> recordedOutOfAdherences, IEnumerable<OpenPeriod> approvedPeriods) =>
			recordedOutOfAdherences.Subtract(approvedPeriods)
				.Select(x => new AdherencePeriod(x.StartTime, x.EndTime))
				.ToArray();

		private static IEnumerable<AdherencePeriod> calculateApprovedPeriods(IEnumerable<OpenPeriod> approvedPeriods) =>
			approvedPeriods
				.Select(x => new AdherencePeriod(x.StartTime, x.EndTime))
				.ToArray();
		
		private IEnumerable<AdherencePeriod> calculateAdjustedToNeutralPeriods(IEnumerable<OpenPeriod> adjustedToNeutralPeriods) =>
			adjustedToNeutralPeriods
				.Select(x => new AdherencePeriod(x.StartTime, x.EndTime))
				.ToArray();

		private static int? calculateSecondsInAdherence(
			DateTimePeriod shift,
			DateTime now,
			IEnumerable<OpenPeriod> outOfAdherences,
			IEnumerable<OpenPeriod> neutralAdherences,
			IEnumerable<OpenPeriod> adjustedToNeutralPeriods,
			IEnumerable<OpenPeriod> approvedPeriods)
		{
			adjustedToNeutralPeriods = adjustedToNeutralPeriods
				.MergeIntersecting()
				.ToArray();

			outOfAdherences = outOfAdherences
				.Subtract(approvedPeriods)
				.Subtract(adjustedToNeutralPeriods)
				.ToArray();

			neutralAdherences = neutralAdherences
				.Subtract(adjustedToNeutralPeriods)
				.Concat(adjustedToNeutralPeriods)
				.Subtract(approvedPeriods)
				.ToArray();

			var timeOut = timeInShift(shift, outOfAdherences);
			var timeNeutral = timeInShift(shift, neutralAdherences);

			var calculateUntil = new[] {now, shift.EndDateTime}.Min();
			var shiftTime = calculateUntil - shift.StartDateTime;
			var timeIn = shiftTime - timeOut - timeNeutral;

			return Convert.ToInt32(timeIn.TotalSeconds);
		}

		private static int? calculateSecondsOutOfAdherence(DateTimePeriod shift, IEnumerable<OpenPeriod> outOfAdherences, IEnumerable<OpenPeriod> neutralAdherences, IEnumerable<OpenPeriod> approvedPeriods)
		{
			outOfAdherences = outOfAdherences
				.Subtract(approvedPeriods)
				.Subtract(neutralAdherences)
				.ToArray();
			var timeOut = timeInShift(shift, outOfAdherences);
			return Convert.ToInt32(timeOut.TotalSeconds);
		}

		private static TimeSpan timeInShift(DateTimePeriod shift, IEnumerable<OpenPeriod> periods)
		{
			var seconds = from p in periods
				let startTime = (p.StartTime ?? DateTime.MinValue).Utc()
				let endTime = (p.EndTime ?? DateTime.MaxValue).Utc()
				let dateTimePeriod = new DateTimePeriod(startTime, endTime)
				let intersection = shift.Intersection(dateTimePeriod)
				where intersection != null
				let t = intersection.Value.EndDateTime - intersection.Value.StartDateTime
				select t.TotalSeconds;
			return TimeSpan.FromSeconds(seconds.Sum());
		}

		private static IEnumerable<OpenPeriod> intersectsWithPeriod(DateTimePeriod period, IEnumerable<OpenPeriod> periods) =>
			(from p in periods
				let startTime = p.StartTime ?? DateTime.MinValue
				let endTime = p.EndTime ?? DateTime.MaxValue
				let endsBeforePeriodStarts = endTime < period.StartDateTime
				let startsAfterPeriodEnds = startTime > period.EndDateTime
				where !endsBeforePeriodStarts && !startsAfterPeriodEnds
				select p
			)
			.ToArray();

		private static DateTime floorToSeconds(DateTime dateTime) => dateTime.Truncate(TimeSpan.FromSeconds(1));

		private static HistoricalChangeAdherence? convertAdherence(EventAdherence? adherence) =>
			adherence != null ? (HistoricalChangeAdherence?) Enum.Parse(typeof(HistoricalChangeAdherence), adherence.ToString()) : null;
	}
}