using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDay
	{
		private Guid _personId;
		private DateTimePeriod _period;
		private IEnumerable<HistoricalChange> _changes;
		private IEnumerable<DateTimePeriod> _outOfAdherences;
		private IEnumerable<DateTimePeriod> _recordedOutOfAdherences;
		private IEnumerable<DateTimePeriod> _approvedPeriods;
		private int? _percentage;

		public void Load(
			Guid personId,
			DateTime now,
			DateTimePeriod period,
			DateTimePeriod? shift,
			IEnumerable<HistoricalChange> changes,
			IEnumerable<HistoricalAdherence> adherences,
			IEnumerable<ApprovedPeriod> approvedPeriods)
		{
			_personId = personId;
			_period = period;
			_changes = loadChanges(changes);
			now = floorToSeconds(now);

			_approvedPeriods = approvedPeriods.Select(a => new DateTimePeriod(a.StartTime, a.EndTime)).ToArray();
			_recordedOutOfAdherences = buildPeriods(HistoricalAdherenceAdherence.Out, adherences, now);

			var recordedNeutralAdherences = buildPeriods(HistoricalAdherenceAdherence.Neutral, adherences, now);
			
			_outOfAdherences = subtractPeriods(_recordedOutOfAdherences, _approvedPeriods);

			var neutralAdherences = subtractPeriods(recordedNeutralAdherences, _approvedPeriods);
			var outOfAhderencesWithinShift = withinShift(shift, _outOfAdherences);
			var neutralAdherencesWithinShift = withinShift(shift, neutralAdherences);

			_percentage = new AdherencePercentageCalculator().Calculate(shift, neutralAdherencesWithinShift, outOfAhderencesWithinShift, now);
		}

		private static IEnumerable<HistoricalChange> loadChanges(IEnumerable<HistoricalChange> changes) =>
			changes
				.GroupBy(y => new
				{
					y.Timestamp,
					y.ActivityName,
					y.ActivityColor,
					y.StateName,
					y.RuleColor,
					y.RuleName,
					y.Adherence
				})
				.Select(x => x.First())
				.ToArray();

		private class periodAccumulator
		{
			public DateTime? StartTime;
			public readonly IList<DateTimePeriod> Periods = new List<DateTimePeriod>();
		}

		private static IEnumerable<DateTimePeriod> buildPeriods(HistoricalAdherenceAdherence adherence, IEnumerable<HistoricalAdherence> adherenceChanges, DateTime now)
		{
			var result = adherenceChanges
				.TransitionsOf(x => x.Adherence)
				.Aggregate(new periodAccumulator(), (a, model) =>
				{
					var timestamp = floorToSeconds(model.Timestamp);
					
					if (model.Adherence == adherence)
						a.StartTime = timestamp;
					else
					{
						if (a.StartTime != null && a.StartTime != timestamp)
							a.Periods.Add(new DateTimePeriod(a.StartTime.Value, timestamp));
						a.StartTime = null;
					}

					return a;
				});
			if (result.StartTime.HasValue && result.StartTime != now)
				result.Periods.Add(new DateTimePeriod(result.StartTime.Value, now));

			return result.Periods;
		}

		private static DateTime floorToSeconds(DateTime dateTime) =>
			dateTime.AddMilliseconds(-dateTime.Millisecond);
		
		private static IEnumerable<DateTimePeriod> subtractPeriods(IEnumerable<DateTimePeriod> periods, IEnumerable<DateTimePeriod> toSubtract) =>
			toSubtract
				.Aggregate(periods, (rs, approved) => rs.Aggregate(Enumerable.Empty<DateTimePeriod>(), (r, recorded) => r.Concat(recorded.Subtract(approved))))
				.ToArray();

		private static IEnumerable<DateTimePeriod> withinShift(DateTimePeriod? shift, IEnumerable<DateTimePeriod> periods) =>
			periods
				.Where(x => shift.HasValue)
				.Where(x => shift.Value.Intersect(x))
				.Select(x => shift.Value.Intersection(x).Value);

		public DateTimePeriod Period() => _period;
		public IEnumerable<HistoricalChange> Changes() => _changes;

		public IEnumerable<OutOfAdherencePeriod> RecordedOutOfAdherences() =>
			_recordedOutOfAdherences.Select(x => new OutOfAdherencePeriod {StartTime = x.StartDateTime, EndTime = x.EndDateTime})
				.ToArray();

		public IEnumerable<ApprovedPeriod> ApprovedPeriods() =>
			_approvedPeriods.Select(x => new ApprovedPeriod {PersonId = _personId, StartTime = x.StartDateTime, EndTime = x.EndDateTime})
				.ToArray();

		public IEnumerable<OutOfAdherencePeriod> OutOfAdherences() =>
			_outOfAdherences
				.Select(x => new OutOfAdherencePeriod {StartTime = x.StartDateTime, EndTime = x.EndDateTime})
				.ToArray();

		public int? Percentage() => _percentage;
	}
}