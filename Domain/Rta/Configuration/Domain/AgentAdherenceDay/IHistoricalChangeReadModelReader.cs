using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelReader
	{
		IEnumerable<HistoricalChangeModel> Read(Guid personId, DateTime startTime, DateTime endTime);
		HistoricalChangeModel ReadLastBefore(Guid personId, DateTime timestamp);
	}
}