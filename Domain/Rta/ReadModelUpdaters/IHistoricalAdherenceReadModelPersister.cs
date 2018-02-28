using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelPersister
	{
		void AddIn(Guid personId, DateTime timestamp);
		void AddNeutral(Guid personId, DateTime timestamp);
		void AddOut(Guid personId, DateTime timestamp);
		void Remove(DateTime until);
	}
}