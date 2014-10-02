using System.Collections.Generic;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AgentsAdherenceMessage
	{	public IEnumerable<AgentAdherenceStateInfo> AgentStates { get; set; }
	}
}
