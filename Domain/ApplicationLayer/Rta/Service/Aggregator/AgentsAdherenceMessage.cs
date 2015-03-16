using System.Collections.Generic;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public class AgentsAdherenceMessage
	{	public IEnumerable<AgentAdherenceStateInfo> AgentStates { get; set; }
	}
}
