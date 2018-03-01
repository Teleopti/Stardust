using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelPersister
	{
		void Persist(HistoricalChangeModel model);
		void Remove(DateTime until);
	}
}