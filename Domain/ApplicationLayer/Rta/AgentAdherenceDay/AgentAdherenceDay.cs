using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDay
	{
		private IEnumerable<HistoricalChange> _changes;
		private IEnumerable<HistoricalAdherence> _adherences;
		private IEnumerable<OutOfAdherencePeriod> _outOfAdherences;

		public void Load(IEnumerable<HistoricalChange> changes, IEnumerable<HistoricalAdherence> adherences)
		{
			_changes = loadChanges(changes);
			_adherences = adherences;
			_outOfAdherences = loadOutOfAdherencePeriods(adherences);
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

		private static IEnumerable<OutOfAdherencePeriod> loadOutOfAdherencePeriods(IEnumerable<HistoricalAdherence> adherences)
		{
			var seed = Enumerable.Empty<OutOfAdherencePeriod>();
			return adherences.Aggregate(seed, (x, model) =>
				{
					if (model.Adherence == HistoricalAdherenceAdherence.Out)
					{
						if (x.IsEmpty(y => y.EndTime == null))
							x = x.Append(new OutOfAdherencePeriod
								{
									StartTime = model.Timestamp
								})
								.ToArray();
					}
					else
					{
						var existing = x.FirstOrDefault(y => y.EndTime == null);
						if (existing != null)
							existing.EndTime = model.Timestamp;
					}

					return x;
				})
				.ToArray();
		}

		public IEnumerable<HistoricalChange> Changes() => _changes;
		public IEnumerable<HistoricalAdherence> Adherences() => _adherences.ToArray();
		public IEnumerable<OutOfAdherencePeriod> OutOfAdherences() => _outOfAdherences;
	}
}