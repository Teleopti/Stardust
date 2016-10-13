using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IHistoricalAdherenceReadModelPersister
	{
		void AddIn(Guid personid, DateTime timestamp);
		void AddNeutral(Guid personid, DateTime timestamp);
		void AddOut(Guid personid, DateTime timestamp);
	}
}