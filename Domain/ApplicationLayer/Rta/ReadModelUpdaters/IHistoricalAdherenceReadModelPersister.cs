using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelPersister
	{
		void AddIn(Guid personid, DateOnly date, DateTime timestamp);
		void AddNeutral(Guid personid, DateOnly date, DateTime timestamp);
		void AddOut(Guid personid, DateOnly date, DateTime timestamp);
	}
}