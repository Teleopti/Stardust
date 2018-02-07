using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Collection;
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
			_outOfAdherences = loadOutOfAdherencePeriods(adherences, now);
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

		private static IEnumerable<OutOfAdherencePeriod> loadOutOfAdherencePeriods(IEnumerable<HistoricalAdherence> adherences, DateTime now)
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

		public DateTimePeriod Period() => _period;
		public IEnumerable<HistoricalChange> Changes() => _changes;
		public IEnumerable<OutOfAdherencePeriod> OutOfAdherences() => _outOfAdherences;
		public int? Percentage() => _percentage;
		public IEnumerable<ApprovedPeriod> ApprovedPeriods() => _approvedPeriods;
	}
}