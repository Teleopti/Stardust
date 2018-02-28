using System;

namespace Teleopti.Ccc.Domain.Rta.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelPersister
	{
		void Persist(HistoricalChange model);
		void Remove(DateTime until);
	}
}