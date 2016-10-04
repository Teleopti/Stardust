using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		void Persist(AgentStateReadModel model);
		void SetDeleted(Guid personId, DateTime expiresAt);
		void DeleteOldRows(DateTime now);
		AgentStateReadModel Get(Guid personId);
		void UpdateAssociation(Guid personId, Guid teamId, Guid? siteId);
	}
}