using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelReader
	{
		IEnumerable<HistoricalAdherence> Read(Guid personId, DateTime startTime, DateTime endTime);
		HistoricalAdherence ReadLastBefore(Guid personId, DateTime timestamp);
	}
}