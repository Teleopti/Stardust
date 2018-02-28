using System;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IAgentStateReadModelPersister
	{
		AgentStateReadModel Load(Guid personId);
		void Update(AgentStateReadModel model);
		
		void UpsertDeleted(Guid personId);
		void UpsertAssociation(AssociationInfo info);
		void UpsertEmploymentNumber(Guid personId, string employmentNumber);
		void UpsertName(Guid personId, string firstName, string lastName);
		void UpdateTeamName(Guid teamId, string name);
		void UpdateSiteName(Guid siteId, string name);
	}
}