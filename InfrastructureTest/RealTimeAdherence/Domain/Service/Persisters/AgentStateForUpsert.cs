using System;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain.Service.Persisters
{
	public static class AgentStatePersisterUpsert
	{
		public static void Upsert(this IAgentStatePersister instance, AgentStateForUpsert model)
		{
			instance.Prepare(new AgentStatePrepare
			{
				PersonId = model.PersonId,
				TeamId = model.TeamId,
				SiteId = model.SiteId,
				BusinessUnitId = model.BusinessUnitId
			}, DeadLockVictim.Yes);
			instance.Update(model);
		}
	}

	public class AgentStateForUpsert : AgentState
	{
		public AgentStateForUpsert()
		{
			PersonId = Guid.NewGuid();
			SnapshotDataSourceId = 0;
			ReceivedTime = DateTime.UtcNow;
		}
	}
}