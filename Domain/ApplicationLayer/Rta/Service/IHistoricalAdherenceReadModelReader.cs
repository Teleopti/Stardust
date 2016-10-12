using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IHistoricalAdherenceReadModelReader
	{
		HistoricalAdherenceReadModel Get(Guid personId, DateOnly date);
	}
}