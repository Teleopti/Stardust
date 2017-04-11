using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalOutOfAdherenceReadModel
	{
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}