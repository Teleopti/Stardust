using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		AgentStateReadModel Load(Guid personId);
		void Persist(AgentStateReadModel model);
		void UpsertDeleted(Guid personId);
		void UpsertAssociation(AssociationInfo info);
		void UpsertEmploymentNumber(Guid personId, string employmentNumber);
		void UpsertName(Guid personId, string firstName, string lastName);
		void UpdateTeamName(Guid teamId, string name);
		void UpdateSiteName(Guid siteId, string name);
	}
}