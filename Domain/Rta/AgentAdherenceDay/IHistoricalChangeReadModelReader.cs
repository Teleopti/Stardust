using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelReader
	{
		IEnumerable<HistoricalChange> Read(Guid personId, DateTime startTime, DateTime endTime);
		HistoricalChange ReadLastBefore(Guid personId, DateTime timestamp);
	}
}