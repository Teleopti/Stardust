using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta
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
				BusinessUnitId = model.BusinessUnitId,
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = model.DataSourceId, UserCode = model.UserCode} }
			}, DeadLockVictim.Yes);
			instance.Update(model);
		}
	}

	public class AgentStateForUpsert : AgentState
	{
		public int DataSourceId { get; set; }
		public string UserCode { get; set; }

		public AgentStateForUpsert()
		{
			DataSourceId = 0;
			UserCode = "usercode";
			ReceivedTime = DateTime.UtcNow;
			SourceId = "0";
		}
	}
}