using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public static class AgentStateReadModelPersisterExtensions
	{
		public static void PersistWithAssociation(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(model.PersonId, model.TeamId.GetValueOrDefault(), model.SiteId, model.BusinessUnitId);
			instance.Persist(model, DeadLockVictim.Yes);
		}
	}
}