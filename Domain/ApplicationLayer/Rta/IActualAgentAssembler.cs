using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActualAgentAssembler
	{
		IEnumerable<AgentStateReadModel> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId);
	}
}