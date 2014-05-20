using System.Collections.Generic;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Messaging.SignalR
{
	public class AgentsAdherenceMessage
	{	public IEnumerable<AgentAdherenceStateInfo> AgentStates { get; set; }
	}
}
