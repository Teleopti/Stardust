using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayWithEventStore : IAgentAdherenceDay
	{
		private Guid _personId;
		private DateTimePeriod _period;
		private IEnumerable<HistoricalChangeModel> _changes;
		private IEnumerable<DateTimePeriod> _outOfAdherences;
		private IEnumerable<DateTimePeriod> _recordedOutOfAdherences;
		private IEnumerable<DateTimePeriod> _approvedPeriods;
		private int? _percentage;

		public void Load(
			Guid personId,
			DateTime now,
			DateTimePeriod period,
			DateTimePeriod? shift,
			IEnumerable<IEvent> events)
		{
			_personId = personId;
			_period = period;
			var allChanges = buildChanges(events);
			_changes = allChanges
				.Where(x => period.ContainsPart(x.Timestamp))
				.ToArray();

			now = floorToSeconds(now);

			_approvedPeriods = buildApprovedPeriods(events);
			_recordedOutOfAdherences = buildPeriods(HistoricalChangeAdherence.Out, allChanges, now);
			_outOfAdherences = subtractPeriods(_recordedOutOfAdherences, _approvedPeriods);

			var recordedNeutralAdherences = buildPeriods(HistoricalChangeAdherence.Neutral, allChanges, now);
			var neutralAdherences = subtractPeriods(recordedNeutralAdherences, _approvedPeriods);
			var outOfAhderencesWithinShift = withinShift(shift, _outOfAdherences);
			var neutralAdherencesWithinShift = withinShift(shift, neutralAdherences);

			_percentage = new AdherencePercentageCalculator().Calculate(shift, neutralAdherencesWithinShift, outOfAhderencesWithinShift, now);
		}

		private static IEnumerable<HistoricalChangeModel> buildChanges(IEnumerable<IEvent> changes) =>
			changes
				.Where(e => e is PersonStateChangedEvent || e is PersonRuleChangedEvent)
				.Select(x => (dynamic) x)
				.Select(@event => new HistoricalChangeModel
				{
					Timestamp = @event.Timestamp,
					StateName = @event.StateName,
					ActivityName = @event.ActivityName,
					ActivityColor = @event.ActivityColor,
					RuleName = @event.RuleName,
					RuleColor = @event.RuleColor,
					Adherence = @event.Adherence == null ? null : (HistoricalChangeAdherence?) Enum.Parse(typeof(HistoricalChangeAdherence), @event.Adherence.ToString())
				})
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

		private static IEnumerable<DateTimePeriod> buildApprovedPeriods(IEnumerable<IEvent> events) =>
			events
				.Where(e => e.GetType() == typeof(PeriodApprovedAsInAdherenceEvent) || e.GetType() == typeof(ApprovedPeriodRemovedEvent))
				.Aggregate(new List<DateTimePeriod>(), (acc, @event) =>
				{
					var eventData = ((IRtaStoredEvent) @event).QueryData();
					var period = new DateTimePeriod(eventData.StartTime.Value, eventData.EndTime.Value);

					if (@event is ApprovedPeriodRemovedEvent)
						acc.Remove(acc.First(a => a.Equals(period)));
					else
						acc.Add(period);

					return acc;
				});

		private class periodAccumulator
		{
			public DateTime? StartTime;
			public readonly IList<DateTimePeriod> Periods = new List<DateTimePeriod>();
		}

		private static IEnumerable<DateTimePeriod> buildPeriods(HistoricalChangeAdherence adherence, IEnumerable<HistoricalChangeModel> adherenceChanges, DateTime now)
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

		private static DateTime floorToSeconds(DateTime dateTime) => dateTime.Truncate(TimeSpan.FromSeconds(1));

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

		public DateTimePeriod Period() => _period;
		public IEnumerable<HistoricalChangeModel> Changes() => _changes;

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