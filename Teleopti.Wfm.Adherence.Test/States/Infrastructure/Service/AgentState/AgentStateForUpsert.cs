﻿using System;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
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

	public class AgentStateForUpsert : Domain.Service.AgentState
	{
		public AgentStateForUpsert()
		{
			PersonId = Guid.NewGuid();
			SnapshotDataSourceId = 0;
			ReceivedTime = DateTime.UtcNow;
		}
	}
}