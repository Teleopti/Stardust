using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.Domain.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelReader
	{
		IEnumerable<HistoricalAdherence> Read(Guid personId, DateTime startTime, DateTime endTime);
		HistoricalAdherence ReadLastBefore(Guid personId, DateTime timestamp);
	}
}