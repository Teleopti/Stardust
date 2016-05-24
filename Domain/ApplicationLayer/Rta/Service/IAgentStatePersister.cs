using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStatePersister
	{
		void Persist(AgentState model);
		void Delete(Guid personId);
		AgentState Get(Guid personId);
		IEnumerable<AgentState> GetAll();
		IEnumerable<AgentState> GetNotInSnapshot(DateTime batchId, string sourceId);
	}
}