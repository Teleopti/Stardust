using System;
using System.Collections.Generic;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Infrastructure
{
	public interface IAgentStateReader
	{
		IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId);
	}
}
