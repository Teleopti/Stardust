using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	public static class AgentStateReadModelPersisterExtensions
	{
		public static void PersistWithAssociation(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
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
			instance.Persist(model);
		}
	}
}