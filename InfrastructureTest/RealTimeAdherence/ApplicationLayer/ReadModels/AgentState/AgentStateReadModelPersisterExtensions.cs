using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	public static class AgentStateReadModelPersisterExtensions
	{
		public static void UpsertToActive(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(new AssociationInfo
			{
				PersonId = model.PersonId,
				BusinessUnitId = model.BusinessUnitId ?? ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = model.SiteId,
				SiteName = model.SiteName,
				TeamId = model.TeamId.GetValueOrDefault(),
				TeamName = model.TeamName
			});
			instance.UpsertName(model.PersonId, model.FirstName, model.LastName);
		}
		
		public static void UpsertToActiveWithState(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(new AssociationInfo
			{
				PersonId = model.PersonId,
				BusinessUnitId = model.BusinessUnitId ?? ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = model.SiteId,
				SiteName = model.SiteName,
				TeamId = model.TeamId.GetValueOrDefault(),
				TeamName = model.TeamName
			});
			instance.UpsertName(model.PersonId, model.FirstName, model.LastName);
			instance.UpdateState(model);
		}

	}
}