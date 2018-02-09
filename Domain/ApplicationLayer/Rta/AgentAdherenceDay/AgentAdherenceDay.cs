using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDay
	{
		private IEnumerable<HistoricalChange> _changes;
		private IEnumerable<OutOfAdherencePeriod> _outOfAdherences;
		private int? _percentage;
		private IEnumerable<ApprovedPeriod> _approvedPeriods;
		private DateTimePeriod _period;
		private IEnumerable<OutOfAdherencePeriod> _recordedOutOfAdherences;

		public void Load(
			DateTime now,
			DateTimePeriod period,
			DateTimePeriod? shift,
			IEnumerable<HistoricalChange> changes,
			IEnumerable<HistoricalAdherence> adherences,
			IEnumerable<ApprovedPeriod> approvedPeriods)
		{
			_period = period;
			_changes = loadChanges(changes);
			_recordedOutOfAdherences = buildRecordedOutOfAdherencePeriods(adherences, now);
			_outOfAdherences = buildOutOfAdherencePeriods(_recordedOutOfAdherences, approvedPeriods);
			_percentage = new AdherencePercentageCalculator().Calculate(shift, adherences, now);
			_approvedPeriods = approvedPeriods;
		}

		private static IEnumerable<HistoricalChange> loadChanges(IEnumerable<HistoricalChange> changes)
		{
			return changes
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
		}

		private static IEnumerable<OutOfAdherencePeriod> buildRecordedOutOfAdherencePeriods(IEnumerable<HistoricalAdherence> adherences, DateTime now)
		{
			var seed = Enumerable.Empty<OutOfAdherencePeriod>();
			return adherences
				.TransitionsOf(x => x.Adherence)
				.Aggregate(seed, (result, model) =>
				{
					if (model.Adherence == HistoricalAdherenceAdherence.Out)
					{
						result = result.Append(new OutOfAdherencePeriod
							{
								StartTime = model.Timestamp,
								EndTime = now
							})
							.ToArray();
					}
					else
					{
						if (result.Any())
							result.Last().EndTime = model.Timestamp;
					}

					return result;
				})
				.ToArray();
		}

		private static IEnumerable<OutOfAdherencePeriod> buildOutOfAdherencePeriods(IEnumerable<OutOfAdherencePeriod> recordedOutOfAdherences, IEnumerable<ApprovedPeriod> approvedPeriods)
		{
			var approveds = approvedPeriods.Select(a => new DateTimePeriod(a.StartTime, a.EndTime));
			var recordeds = recordedOutOfAdherences.Select(a => new DateTimePeriod(a.StartTime, a.EndTime));

			return approveds
				.Aggregate(recordeds, (rs, approved) => rs.Aggregate(Enumerable.Empty<DateTimePeriod>(), (r, recorded) => r.Concat(recorded.Subtract(approved))))
				.Select(r => new OutOfAdherencePeriod {StartTime = r.StartDateTime, EndTime = r.EndDateTime})
				.ToArray();
		}

		public DateTimePeriod Period() => _period;
		public IEnumerable<HistoricalChange> Changes() => _changes;
		public IEnumerable<OutOfAdherencePeriod> OutOfAdherences() => _outOfAdherences;
		public int? Percentage() => _percentage;
		public IEnumerable<ApprovedPeriod> ApprovedPeriods() => _approvedPeriods;
		public IEnumerable<OutOfAdherencePeriod> RecordedOutOfAdherences() => _recordedOutOfAdherences;
	}
}