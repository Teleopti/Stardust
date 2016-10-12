using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelReader
	{
		HistoricalAdherenceReadModel Read(Guid personId, DateOnly date);
	}
}