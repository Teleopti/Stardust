using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public static class AgentStateReadModelPersisterExtensions
	{
		public static void PersistWithAssociation(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(new AssociationInfo()
			{
				PersonId = model.PersonId,
				BusinessUnitId = model.BusinessUnitId,
				SiteId = model.SiteId,
				SiteName = model.SiteName,
				TeamId = model.TeamId.GetValueOrDefault(),
				TeamName = model.TeamName
			});
			instance.Persist(model);
		}
	}
}