using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		AgentStateReadModel Load(Guid personId);
		void Persist(AgentStateReadModel model);
		void SetDeleted(Guid personId, DateTime expiresAt);
		void DeleteOldRows(DateTime now);
		void UpsertAssociation(Guid personId, Guid teamId, Guid? siteId, Guid? businessUnitId);
	}
}