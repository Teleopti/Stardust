using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayWithLateForWork : IAgentAdherenceDay
	{
		private readonly Guid _personId;
		private DateTimePeriod _period;
		private readonly DateTimePeriod? _shift;
		private readonly DateTime _now;

		private IList<AdherencePeriod> _recordedOutOfAdherences = new List<AdherencePeriod>();
		private IList<AdherencePeriod> _recordedNeutralAdherences = new List<AdherencePeriod>();
		private readonly IList<HistoricalChangeModel> _changes = new List<HistoricalChangeModel>();
		private readonly IList<DateTimePeriod> _approvedPeriods = new List<DateTimePeriod>();

		public AgentAdherenceDayWithLateForWork(Guid personId, DateTimePeriod period, DateTimePeriod? shift, DateTime now)
		{
			_personId = personId;
			_period = period;
			_shift = shift;
			_now = floorToSeconds(now);
		}

		public void Apply(PersonStateChangedEvent @event) => applySolidProof(@event);

		public void Apply(PersonRuleChangedEvent @event) => applySolidProof(@event);

		public void Apply(PersonInAdherenceAfterLateForWorkEvent @event)
		{
			var model = applySolidProof(@event);
			if (model != null)
				model.LateForWork = string.Format(UserTexts.Resources.LateXMinutes, new DateTimePeriod(@event.ShiftStart, @event.Timestamp).ElapsedTime().TotalMinutes);
		}

		public void Apply(PeriodApprovedAsInAdherenceEvent @event) =>
			_approvedPeriods.Add(new DateTimePeriod(@event.StartTime, @event.EndTime));

		public void Apply(ApprovedPeriodRemovedEvent @event) =>
			_approvedPeriods.Remove(new DateTimePeriod(@event.StartTime, @event.EndTime));

		public void ApplyDone()
		{
			_recordedOutOfAdherences = filterTinyPeriods(_recordedOutOfAdherences);
			_recordedNeutralAdherences = filterTinyPeriods(_recordedNeutralAdherences);
		}

		private static IList<AdherencePeriod> filterTinyPeriods(IEnumerable<AdherencePeriod> periods) 
			=> periods
			.Select(x => new AdherencePeriod(floorToSeconds(x.StartTime), floorToSeconds(x.EndTime)))
			.Where(x => x.EndTime - x.StartTime >= TimeSpan.FromSeconds(1))
			.ToList();

		private HistoricalChangeModel applySolidProof(ISolidProof @event)
		{
			applyAdherencePeriod(_recordedOutOfAdherences, @event.Timestamp, @event.Adherence, EventAdherence.Out);
			applyAdherencePeriod(_recordedNeutralAdherences, @event.Timestamp, @event.Adherence, EventAdherence.Neutral);

			var isInitialAdherenceEvent = @event.Timestamp < _period.StartDateTime;
			if (isInitialAdherenceEvent)
				return null;

			var model = _changes.LastOrDefault();
			if (model == null || !isSameProof(model, @event))
			{
				model = new HistoricalChangeModel();
				_changes.Add(model);
			}

			model.Timestamp = @event.Timestamp;
			model.StateName = @event.StateName;
			model.ActivityName = @event.ActivityName;
			model.ActivityColor = @event.ActivityColor;
			model.RuleName = @event.RuleName;
			model.RuleColor = @event.RuleColor;
			model.Adherence = convertAdherence(@event.Adherence);

			return model;
		}

		private static bool isSameProof(HistoricalChangeModel model, ISolidProof @event) =>
			model.Timestamp == @event.Timestamp &&
			model.ActivityName == @event.ActivityName &&
			model.ActivityColor == @event.ActivityColor &&
			model.StateName == @event.StateName &&
			model.RuleColor == @event.RuleColor &&
			model.RuleName == @event.RuleName &&
			model.Adherence == convertAdherence(@event.Adherence);

		private void applyAdherencePeriod(ICollection<AdherencePeriod> periodRecords, DateTime time, EventAdherence? adherence, EventAdherence adherencePeriod)
		{
			var lastPeriod = periodRecords.LastOrDefault();
			if (adherence == adherencePeriod)
			{
				if (lastPeriod == null || periodIsClosed(lastPeriod))
					periodRecords.Add(new AdherencePeriod {StartTime = time, EndTime = _now});
			}
			else if (lastPeriod != null)
			{
				if (periodIsOpen(lastPeriod))
					lastPeriod.EndTime = time;
			}
		}

		private bool periodIsOpen(AdherencePeriod period) => period != null && period.EndTime == _now;
		private bool periodIsClosed(AdherencePeriod period) => period.EndTime != _now;

		public DateTimePeriod Period() => _period;
		public IEnumerable<HistoricalChangeModel> Changes() => _changes;

		public IEnumerable<AdherencePeriod> RecordedOutOfAdherences() => _recordedOutOfAdherences.ToArray();

		public IEnumerable<ApprovedPeriod> ApprovedPeriods() =>
			_approvedPeriods.Select(x => new ApprovedPeriod {PersonId = _personId, StartTime = x.StartDateTime, EndTime = x.EndDateTime})
				.ToArray();

		public IEnumerable<AdherencePeriod> OutOfAdherences() =>
			subtractPeriods(_recordedOutOfAdherences.Select(x => new DateTimePeriod(x.StartTime, x.EndTime)), _approvedPeriods)
				.Select(x => new AdherencePeriod(x.StartDateTime, x.EndDateTime))
				.ToArray();

		public int? Percentage()
		{
			var neutralAdherences = subtractPeriods(_recordedNeutralAdherences.Select(x => new DateTimePeriod(x.StartTime, x.EndTime)), _approvedPeriods);
			var outOfAhderencesWithinShift = withinShift(_shift, OutOfAdherences().Select(x => new DateTimePeriod(x.StartTime, x.EndTime)));
			var neutralAdherencesWithinShift = withinShift(_shift, neutralAdherences);
			return new AdherencePercentageCalculator().Calculate(_shift, neutralAdherencesWithinShift, outOfAhderencesWithinShift, _now);
		}

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


//		public void Load(
//			Guid personId,
//			DateTime now,
//			DateTimePeriod period,
//			DateTimePeriod? shift,
//			IEnumerable<IEvent> events)
//		{
//			_personId = personId;
//			_period = period;
//			var allChanges = buildChanges(events);
//			_changes = allChanges
//				.Where(x => period.ContainsPart(x.Timestamp))
//				.ToArray();
//
//			now = floorToSeconds(now);
//
//			_approvedPeriods = buildApprovedPeriods(events);
//			_recordedOutOfAdherences = buildPeriods(HistoricalChangeAdherence.Out, allChanges, now);
//			_outOfAdherences = subtractPeriods(_recordedOutOfAdherences, _approvedPeriods);
//
//			var recordedNeutralAdherences = buildPeriods(HistoricalChangeAdherence.Neutral, allChanges, now);
//			var neutralAdherences = subtractPeriods(recordedNeutralAdherences, _approvedPeriods);
//			var outOfAhderencesWithinShift = withinShift(shift, _outOfAdherences);
//			var neutralAdherencesWithinShift = withinShift(shift, neutralAdherences);
//
//			_percentage = new AdherencePercentageCalculator().Calculate(shift, neutralAdherencesWithinShift, outOfAhderencesWithinShift, now);
//		}
//
//		private static IEnumerable<HistoricalChangeModel> buildChanges(IEnumerable<IEvent> changes)
//		{
//			var lateForWorkEvent = changes
//				.OfType<PersonInAdherenceAfterLateForWorkEvent>()
//				.SingleOrDefault();
//
//			string lateForWork = null;
//			if (lateForWorkEvent != null)
//			{
//				var period = new DateTimePeriod(lateForWorkEvent.ShiftStart, lateForWorkEvent.Timestamp);
//				lateForWork = string.Format(UserTexts.Resources.LateXMinutes, period.ElapsedTime().TotalMinutes);
//			}
//
//			var filteredEvents = changes
//				.Where(e => e is PersonStateChangedEvent || e is PersonRuleChangedEvent);
//
//			if (changes.Count() == 1 && lateForWorkEvent != null)
//				filteredEvents = changes.OfType<PersonInAdherenceAfterLateForWorkEvent>();
//
//			return
//				filteredEvents
//					.Select(x => (dynamic) x)
//					.Select(@event => new HistoricalChangeModel
//					{
//						Timestamp = @event.Timestamp,
//						StateName = @event.StateName,
//						ActivityName = @event.ActivityName,
//						ActivityColor = @event.ActivityColor,
//						RuleName = @event.RuleName,
//						RuleColor = @event.RuleColor,
//						Adherence = @event.Adherence == null ? null : (HistoricalChangeAdherence?) Enum.Parse(typeof(HistoricalChangeAdherence), @event.Adherence.ToString()),
//						LateForWork = lateForWorkEvent?.Timestamp == @event.Timestamp ? lateForWork : null
//					})
//					.GroupBy(y => new
//					{
//						y.Timestamp,
//						y.ActivityName,
//						y.ActivityColor,
//						y.StateName,
//						y.RuleColor,
//						y.RuleName,
//						y.Adherence
//					})
//					.Select(x => x.First())
//					.ToArray();
//		}
//
//
//		private static IEnumerable<DateTimePeriod> buildApprovedPeriods(IEnumerable<IEvent> events) =>
//			events
//				.Where(e => e.GetType() == typeof(PeriodApprovedAsInAdherenceEvent) || e.GetType() == typeof(ApprovedPeriodRemovedEvent))
//				.Aggregate(new List<DateTimePeriod>(), (approvedPeriods, @event) =>
//				{
//					var eventData = ((IRtaStoredEvent) @event).QueryData();
//					var period = new DateTimePeriod(eventData.StartTime.Value, eventData.EndTime.Value);
//
//					if (@event is ApprovedPeriodRemovedEvent)
//						approvedPeriods.Remove(approvedPeriods.FirstOrDefault(a => a.Equals(period)));
//					else
//						approvedPeriods.Add(period);
//
//					return approvedPeriods;
//				});
//
//		private class periodAccumulator
//		{
//			public DateTime? StartTime;
//			public readonly IList<DateTimePeriod> Periods = new List<DateTimePeriod>();
//		}
//
//		private static IEnumerable<DateTimePeriod> buildPeriods(HistoricalChangeAdherence adherence, IEnumerable<HistoricalChangeModel> adherenceChanges, DateTime now)
//		{
//			var result = adherenceChanges
//				.TransitionsOf(x => x.Adherence)
//				.Aggregate(new periodAccumulator(), (a, model) =>
//				{
//					var timestamp = floorToSeconds(model.Timestamp);
//
//					if (model.Adherence == adherence)
//						a.StartTime = timestamp;
//					else
//					{
//						if (a.StartTime != null && a.StartTime != timestamp)
//							a.Periods.Add(new DateTimePeriod(a.StartTime.Value, timestamp));
//						a.StartTime = null;
//					}
//
//					return a;
//				});
//			if (result.StartTime.HasValue && result.StartTime != now)
//				result.Periods.Add(new DateTimePeriod(result.StartTime.Value, now));
//
//			return result.Periods;
//		}
//
//		private static DateTime floorToSeconds(DateTime dateTime) => dateTime.Truncate(TimeSpan.FromSeconds(1));
//
//		private static IEnumerable<DateTimePeriod> subtractPeriods(IEnumerable<DateTimePeriod> periods, IEnumerable<DateTimePeriod> toSubtract) =>
//			toSubtract
//				.Aggregate(periods, (rs, approved) => rs.Aggregate(Enumerable.Empty<DateTimePeriod>(), (r, recorded) => r.Concat(recorded.Subtract(approved))))
//				.ToArray();
//
//		private static IEnumerable<DateTimePeriod> withinShift(DateTimePeriod? shift, IEnumerable<DateTimePeriod> periods) =>
//			periods
//				.Where(x => shift.HasValue)
//				.Where(x => shift.Value.Intersect(x))
//				.Select(x => shift.Value.Intersection(x).Value)
//				.ToArray();
	}
}