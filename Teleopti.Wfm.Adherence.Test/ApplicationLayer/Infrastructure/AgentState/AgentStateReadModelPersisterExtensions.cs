using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.Infrastructure.AgentState
{
	public static class AgentStateReadModelPersisterExtensions
	{
		public static void Upsert(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(new AssociationInfo
			{
				PersonId = model.PersonId,
				BusinessUnitId = model.BusinessUnitId ?? ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = model.SiteId,
				SiteName = model.SiteName,
				TeamId = model.TeamId.GetValueOrDefault(),
				TeamName = model.TeamName,
				FirstName = model.FirstName,
				LastName = model.LastName
			});
		}
		
		public static void UpsertWithState(this IAgentStateReadModelPersister instance, AgentStateReadModel model)
		{
			instance.UpsertAssociation(new AssociationInfo
			{
				PersonId = model.PersonId,
				BusinessUnitId = model.BusinessUnitId ?? ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = model.SiteId,
				SiteName = model.SiteName,
				TeamId = model.TeamId.GetValueOrDefault(),
				TeamName = model.TeamName,
				FirstName = model.FirstName,
				LastName = model.LastName,
			});
			instance.UpdateState(model);
		}

	}
}