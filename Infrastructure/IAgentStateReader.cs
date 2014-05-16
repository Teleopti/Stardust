using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Infrastructure
{
	public interface IAgentStateReader
	{
		IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId);
	}
}
