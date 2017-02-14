using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		AgentStateReadModel Load(Guid personId);
		void Persist(AgentStateReadModel model);
		void UpsertDeleted(Guid personId, DateTime expiresAt);
		void DeleteOldRows(DateTime now);
		void UpsertAssociation(AssociationInfo info);
		void UpsertEmploymentNumber(Guid personId, string employmentNumber, DateTime? expiresAt);
		void UpsertName(Guid personId, string firstName, string lastName, DateTime? expiresAt);
		void UpdateTeamName(Guid teamId, string name);
		void UpdateSiteName(Guid siteId, string name);
	}
}