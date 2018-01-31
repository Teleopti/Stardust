using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDayLoader
	{
		private readonly IHistoricalChangeReadModelReader _changes;
		private readonly IHistoricalAdherenceReadModelReader _adherences;

		public AgentAdherenceDayLoader(IHistoricalChangeReadModelReader changes, IHistoricalAdherenceReadModelReader adherences)
		{
			_changes = changes;
			_adherences = adherences;
		}

		public AgentAdherenceDay Load(
			Guid personId,
			DateOnly date,
			DateTime startTime,
			DateTime endTime)
		{
			var changes = _changes.Read(personId, startTime, endTime);
			var adherences = new[] {_adherences.ReadLastBefore(personId, startTime)}
				.Concat(_adherences.Read(personId, startTime, endTime))
				.Where(x => x != null);
			var obj = new AgentAdherenceDay();
			obj.Load(changes, adherences);
			return obj;
		}
	}
}