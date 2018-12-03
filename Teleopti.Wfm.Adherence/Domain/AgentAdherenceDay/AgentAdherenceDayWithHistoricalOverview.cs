using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;

using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayWithHistoricalOverview : IAgentAdherenceDay
	{
		private readonly Guid _personId;
		private readonly DateTimePeriod _period;
		private readonly DateTimePeriod? _shift;
		private readonly DateTime _now;

		private IList<AdherencePeriod> _recordedOutOfAdherences = new List<AdherencePeriod>();
		private IList<AdherencePeriod> _recordedNeutralAdherences = new List<AdherencePeriod>();
		private readonly IList<HistoricalChangeModel> _changes = new List<HistoricalChangeModel>();
		private readonly IList<DateTimePeriod> _approvedPeriods = new List<DateTimePeriod>();
		private IEnumerable<DateTimePeriod> _outOfAhderencesWithinShift;
		private IEnumerable<DateTimePeriod> _neutralAdherencesWithinShift;
		private int? _adherencePercentage;
		private int? _secondsInAdherence;
		private int? _secondsOutOfAdherence;

		public AgentAdherenceDayWithHistoricalOverview(Guid personId, DateTimePeriod period, DateTimePeriod? shift, DateTime now)
		{
			_personId = personId;
			_period = period;
			_shift = shift;
			_now = floorToSeconds(now);
		}

		public void Apply(PersonShiftStartEvent @event)
		{
		}

		public void Apply(PersonShiftEndEvent @event)
		{
		}
		
		public void Apply(PersonStateChangedEvent @event) => applySolidProof(@event);

		public void Apply(PersonRuleChangedEvent @event) => applySolidProof(@event);

		public void Apply(PersonArrivedLateForWorkEvent @event)
		{
			var model = applySolidProof(@event);
			if (model != null)
				model.LateForWork = string.Format(Ccc.UserTexts.Resources.LateXMinutes, Math.Round(new DateTimePeriod(@event.ShiftStart, @event.Timestamp).ElapsedTime().TotalMinutes));
		}

		public void Apply(PeriodApprovedAsInAdherenceEvent @event) =>
			_approvedPeriods.Add(new DateTimePeriod(@event.StartTime, @event.EndTime));

		public void Apply(ApprovedPeriodRemovedEvent @event) =>
			_approvedPeriods.Remove(new DateTimePeriod(@event.StartTime, @event.EndTime));

		public void ApplyDone()
		{
			closeOpenPeriod(_recordedOutOfAdherences);
			closeOpenPeriod(_recordedNeutralAdherences);

			_recordedOutOfAdherences = removeTinyPeriods(_recordedOutOfAdherences);
			_recordedOutOfAdherences = mergePeriods(_recordedOutOfAdherences);

			_recordedNeutralAdherences = removeTinyPeriods(_recordedNeutralAdherences);
			_recordedNeutralAdherences = mergePeriods(_recordedNeutralAdherences);

			_outOfAhderencesWithinShift = withinShift(_shift, OutOfAdherences().Select(x => new DateTimePeriod(x.StartTime.Value, x.EndTime.Value)));
			_neutralAdherencesWithinShift = withinShift(_shift, neutralAdherencePeriods());

			calculateAdherence(_shift, _neutralAdherencesWithinShift, _outOfAhderencesWithinShift, _now);
		}

		private HistoricalChangeModel applySolidProof(ISolidProof @event)
		{
			applyAdherencePeriod(_recordedOutOfAdherences, @event.Timestamp, @event.Adherence, EventAdherence.Out);
			applyAdherencePeriod(_recordedNeutralAdherences, @event.Timestamp, @event.Adherence, EventAdherence.Neutral);

			var isEventBeforePeriod = @event.Timestamp < _period.StartDateTime;
			if (isEventBeforePeriod)
				return null;

			var lastProof = _changes.LastOrDefault();
			if (lastProof != null && !isSameProof(lastProof, @event))
				lastProof.Duration = floorToSeconds(@event.Timestamp).Subtract(floorToSeconds(lastProof.Timestamp)).ToString();

			var needNewProof = (lastProof == null || !isSameProof(lastProof, @event));
			if (needNewProof)
			{
				var proof = createProof(@event);
				_changes.Add(proof);
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

		private void applyAdherencePeriod(ICollection<AdherencePeriod> periods, DateTime time, EventAdherence? adherence, EventAdherence adherenceType)
		{
			var period = periods.LastOrDefault();
			if (adherence == adherenceType)
			{
				if (period == null || period.EndTime != null)
					periods.Add(new AdherencePeriod {StartTime = time});
			}
			else
			{
				if (period != null && period.EndTime == null)
					period.EndTime = time;
			}
		}

		private void closeOpenPeriod(IEnumerable<AdherencePeriod> periods)
		{
			var period = periods.LastOrDefault();
			if (period != null && period.EndTime == null)
				period.EndTime = _now;
		}

		private static IList<AdherencePeriod> mergePeriods(IEnumerable<AdherencePeriod> periods)
			=> periods.Aggregate(new List<AdherencePeriod>(), (acc, i) =>
			{
				var lastPeriod = acc.LastOrDefault();
				if (lastPeriod != null && lastPeriod.EndTime.Value == i.StartTime)
					lastPeriod.EndTime = i.EndTime;
				else
					acc.Add(i);
				return acc;
			});

		private static IList<AdherencePeriod> removeTinyPeriods(IEnumerable<AdherencePeriod> periods)
			=> periods
				.Select(x => new AdherencePeriod(floorToSeconds(x.StartTime.Value), floorToSeconds(x.EndTime.Value)))
				.Where(x => x.EndTime - x.StartTime >= TimeSpan.FromSeconds(1))
				.ToList();

		public DateTimePeriod DisplayPeriod() => _period;

		public IEnumerable<HistoricalChangeModel> Changes() => _changes;

		public IEnumerable<AdherencePeriod> RecordedOutOfAdherences() => _recordedOutOfAdherences.ToArray();

		public IEnumerable<ApprovedPeriod> ApprovedPeriods() =>
			_approvedPeriods.Select(x => new ApprovedPeriod {PersonId = _personId, StartTime = x.StartDateTime, EndTime = x.EndDateTime})
				.ToArray();

		public IEnumerable<AdherencePeriod> OutOfAdherences() =>
			subtractPeriods(_recordedOutOfAdherences.Select(x => new DateTimePeriod(x.StartTime.Value, x.EndTime.Value)), _approvedPeriods)
				.Select(x => new AdherencePeriod(x.StartDateTime, x.EndDateTime))
				.ToArray();

		public int? Percentage() => _adherencePercentage;

		public int? SecondsInAdherence() => _secondsInAdherence;

		public int? SecondsOutOfAdherence() => _secondsOutOfAdherence;

		private IEnumerable<DateTimePeriod> neutralAdherencePeriods()
		{
			return subtractPeriods(_recordedNeutralAdherences.Select(x => new DateTimePeriod(x.StartTime.Value, x.EndTime.Value)), _approvedPeriods).ToArray();
		}

		private void calculateAdherence(DateTimePeriod? shift, IEnumerable<DateTimePeriod> neutralPeriods, IEnumerable<DateTimePeriod> outPeriods, DateTime now)
		{
			if (!shift.HasValue)
				return;

			var calculateUntil = new[] {now, shift.Value.EndDateTime}.Min();

			var shiftTime = calculateUntil - shift.Value.StartDateTime;

			var timeNeutral = time(neutralPeriods);
			var timeToAdhere = shiftTime - timeNeutral;
			if (timeToAdhere == TimeSpan.Zero)
				return;

			var timeOut = time(outPeriods);
			var timeIn = shiftTime - timeOut - timeNeutral;

			_secondsInAdherence = Convert.ToInt32(timeIn.TotalSeconds);
			_secondsOutOfAdherence = Convert.ToInt32(timeOut.TotalSeconds);
			_adherencePercentage = Convert.ToInt32((timeIn.TotalSeconds / timeToAdhere.TotalSeconds) * 100);
		}

		private static TimeSpan time(IEnumerable<DateTimePeriod> periods) =>
			TimeSpan.FromSeconds(periods.Sum(x => (x.EndDateTime - x.StartDateTime).TotalSeconds));

		//difficult to grasp the linq
		private static IEnumerable<DateTimePeriod> subtractPeriods(IEnumerable<DateTimePeriod> periods, IEnumerable<DateTimePeriod> toSubtract) =>
			toSubtract
				.Aggregate(periods, (rs, approved) => rs.Aggregate(Enumerable.Empty<DateTimePeriod>(), (r, recorded) => r.Concat(recorded.Subtract(approved))))
				.ToArray();

		private static IEnumerable<DateTimePeriod> withinShift(DateTimePeriod? shift, IEnumerable<DateTimePeriod> periods) =>
			periods
				.Where(x => shift.HasValue)
				.Where(x => shift.Value.Intersect(x))
				.Select(x => shift.Value.Intersection(x).Value)
				.ToArray();

		private static DateTime floorToSeconds(DateTime dateTime) => dateTime.Truncate(TimeSpan.FromSeconds(1));

		private static HistoricalChangeAdherence? convertAdherence(EventAdherence? adherence) =>
			adherence != null ? (HistoricalChangeAdherence?) Enum.Parse(typeof(HistoricalChangeAdherence), adherence.ToString()) : null;
	}
}