using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public interface IHistoricalChangeReadModelPersister
	{
		void Persist(HistoricalChange model);
		void Remove(DateTime until);
	}
}