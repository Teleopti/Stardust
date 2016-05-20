using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		void Persist(AgentStateReadModel model);
		void Delete(Guid personId);
	}
}