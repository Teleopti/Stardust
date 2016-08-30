using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		void Persist(AgentStateReadModel model);
		void Delete(Guid personId);
		AgentStateReadModel Get(Guid personId);
		void UpdateAssociation(Guid personId, Guid teamId, Guid? siteId);
	}
}