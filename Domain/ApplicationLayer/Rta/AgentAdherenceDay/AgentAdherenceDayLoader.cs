using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AgentAdherenceDayLoader
	{
		private readonly INow _now;
		private readonly IHistoricalChangeReadModelReader _changes;
		private readonly IHistoricalAdherenceReadModelReader _adherences;

		public AgentAdherenceDayLoader(
			INow now,
			IHistoricalChangeReadModelReader changes, 
			IHistoricalAdherenceReadModelReader adherences)
		{
			_now = now;
			_changes = changes;
			_adherences = adherences;
		}

		public AgentAdherenceDay Load(
			Guid personId,
			DateOnly date,
			DateTime startTime,
			DateTime endTime,
			DateTime? shiftStartTime,
			DateTime? shiftEndTime)
		{
			var changes = _changes.Read(personId, startTime, endTime);
			var adherences = new[] {_adherences.ReadLastBefore(personId, startTime)}
				.Concat(_adherences.Read(personId, startTime, endTime))
				.Where(x => x != null);
			var obj = new AgentAdherenceDay();
			obj.Load(
				_now.UtcDateTime(), 
				changes, 
				adherences,
				shiftStartTime,
				shiftEndTime
				);
			return obj;
		}
	}
}