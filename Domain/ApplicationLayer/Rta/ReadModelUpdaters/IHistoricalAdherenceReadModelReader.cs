using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelReader
	{
		IEnumerable<HistoricalAdherenceReadModel> Read(Guid personId, DateTime startTime, DateTime endTime);
		HistoricalAdherenceReadModel ReadLastBefore(Guid personId, DateTime timestamp);
	}

	public class HistoricalAdherenceReadModel
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public HistoricalAdherenceReadModelAdherence Adherence { get; set; }
	}

	public enum HistoricalAdherenceReadModelAdherence
	{
		In,
		Neutral,
		Out
	}
}