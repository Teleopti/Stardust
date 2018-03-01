using System;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelPersister
	{
		void Persist(HistoricalChangeModel model);
		void Remove(DateTime until);
	}
}