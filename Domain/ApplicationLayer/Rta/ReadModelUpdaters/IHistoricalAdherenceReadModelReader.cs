using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelReader
	{
		HistoricalAdherenceReadModel Read(Guid personId, DateTime startTime, DateTime endTime);
	}
}