using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class HistoricalAdherence
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public HistoricalAdherenceAdherence Adherence { get; set; }
	}
}