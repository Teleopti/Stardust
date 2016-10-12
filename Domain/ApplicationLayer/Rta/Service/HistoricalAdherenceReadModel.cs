using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceReadModel
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public IEnumerable<HistoricalOutOfAdherenceReadModel> OutOfAdherences { get; set; }
	}

	public class HistoricalOutOfAdherenceReadModel
	{
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}