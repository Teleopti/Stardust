using System;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IAgentStateReadModelPersister
	{
		AgentStateReadModel Load(Guid personId);
		void UpdateState(AgentStateReadModel model);
		
		void UpsertNoAssociation(Guid personId);
		void UpsertAssociation(AssociationInfo info);
		void UpsertEmploymentNumber(Guid personId, string employmentNumber);
		void UpsertName(Guid personId, string firstName, string lastName);
		void UpdateTeamName(Guid teamId, string name);
		void UpdateSiteName(Guid siteId, string name);
	}
}